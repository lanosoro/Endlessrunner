using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public enum level_type
{
    Menu = 0, //paused, game menu or in game prompts that pause the game. 
    Gameplay = 1, //plays game from current data, unpaused
    GameOver = 2, //resets game data, gives you game over screen. Paused

}
//simple holder for each segement to let us know what level and what object. 
[System.Serializable]
public class levelsegment_levels
{
    [SerializeField]
    public int levelseg=0;
    [SerializeField]
    public int objseg=0;
    [SerializeField]
    public int segment_length = 15; //z length to tile the segements
  
}

[System.Serializable]
public class levels
{
    [SerializeField]
    [NonReorderable]
    public List<levelsegment_levels> Segments;
    [SerializeField]
    public string LevelName="level";
    [SerializeField]
    public level_type levtype;
  
    //[SerializeField]
    //public int time = -10; //updates ++, number of frames played
    [SerializeField]
    public int speed = 30; //updates ++, number of frames played

}
public class EndlessRunner_Levels : MonoBehaviour
{
    public TMPro.TMP_Text timetext;
   // [SerializeField]
  //  public int seg_count = 6; //how many to make in front of player
    public Transform contentholder; //where we spawn level segments
    public Transform dynamic;
    public TMPro.TMP_Text levelname;
    public GameObject[] LevelSegments; //prefabs of level segments
    public GameObject[] ObjectSegments; //prefabs of object segments
    public bool autostart = true; //do we start at time = 0
    public int speed = 50; //speed of trackmovement globally controls all segment movement. 
   // public bool levelclear = false;
    [NonReorderable]
    public List<levels> levelsmaster;  //class at bottom holding int int level id and object id
    //public int levelspeed =0;
    private bool ready = false;  //make sure stuff is ready before we start internal 
    private int current_order = 0; //which levelorder index are we using, use length of it to trigger cycle start over at 0
    //levels
    public int startinglevel=0;
    public int currentlevel=0;
    private int currentsegmentlength=0;
    //unity pooling
    public bool collectionChecks = true;
    public int maxPoolSize = 10;
    public List<IObjectPool<GameObject>> level_Pools;
    public List<IObjectPool<GameObject>> object_Pools;
    public int pooltype = 0; //0 = level pool 1 = object pool, I share the same pooling system with multiple types
    private int level_type = 0; //which prefab to use
    private int object_type = 0;//which prefab to use
  
    private void Awake()
    {
        levelname.text = levelsmaster[startinglevel].LevelName;
        //init
        level_Pools = new List<IObjectPool<GameObject>>();
        object_Pools = new List<IObjectPool<GameObject>>();
        currentlevel = startinglevel;
        speed = levelsmaster[startinglevel].speed;
        //level seg type = 0, loading the prefabs into the pools. 
        pooltype = 0;
        foreach (var g in LevelSegments)
        {
            //add the road tile script for movement of the world
            roadtile_Levels rt = g.AddComponent<roadtile_Levels>();         
            rt.master = this; //link this script to this master so we can commuinicate back. 
            rt.setactive = false;          //don't move yet
            level_Pools.Add( new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize));
            level_type++;
        }
        //object seg type = 0, loading the prefabs into the pools. 
        pooltype = 1;
        foreach (var g in ObjectSegments)
        { 
            object_Pools.Add(new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize));
           
            object_type++;
        }
        //program add level 4
        if (levelsmaster.Count < 4)
            levelsmaster.Add(levelsmaster[levelsmaster.Count - 1]); //clone the last one in the list to add a new one. 



    }
    //happens every physics step, you can control in time in project settings
    private void FixedUpdate()
    {

        //levelsmaster[currentlevel].time++;//we increment time
        //timetext.text = "Time: " + levelsmaster[currentlevel].time.ToString();
        
        if (!ready) //starts off
            if (autostart)// & levelsmaster[currentlevel].time > -1) //time == 0 and we want to autostart
            {
                for (int x = 0; x < levelsmaster[currentlevel].Segments.Count; x++) //for each active segement
                {
                    
                    currentsegmentlength += levelsmaster[currentlevel].Segments[current_order].segment_length;
                    spawnnewsegement(levelsmaster[currentlevel].Segments[current_order], currentsegmentlength); //create a new active segement for both level and object
                    //increment the current_order but check to see if we need to loop it back to 0
                    if (x < levelsmaster[currentlevel].Segments.Count-1)
                        orderplus();
                }
               

                ready = true; // now it is true we will not repeat this step. 
            }
     
    }

    //required by object pooler, I use type here to make what object I need, and level / object type to get the segment. 
    //so I was able to reuse the pooler instead of make a new one per item. Just have 2 types supported now but more could be added. 
    int activepooled = 0;
    GameObject CreatePooledItem()
    {

      
        if (pooltype == 0)           
          return   Instantiate(LevelSegments[level_type], contentholder, false);
        else 
          return  Instantiate(ObjectSegments[object_type], contentholder, false);
       
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(GameObject system)
    {
        activepooled--;
        system.SetActive(false);
        if (pooltype == 1)
        system.transform.parent = dynamic;
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(GameObject system)
    {
        activepooled++;
        system.SetActive(true);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(GameObject system)
    {
        Destroy(system);
    }
  
  
    //this is the reusable function to make a new level segment at x dis
     GameObject spawnnewsegement(levelsegment_levels ls, float dis)
    {
        //spawn the level
        pooltype = 0;
        //validate we have not hit the end of the prefabs
        if (ls.levelseg >= LevelSegments.Length)
            ls.levelseg = 0;
        //levelsegment class has level and object seg data from what we enter on the unity edit window. 
        level_type =  ls.levelseg;

        GameObject lvlseg = level_Pools[ls.levelseg].Get();//this spawns the object. (object spawned has to match the prefab that pool was created with. 
        
        //now we can set the game object postion 
        lvlseg.transform.localPosition= new Vector3(0, 0, dis);

        //if object segement
        if (ls.objseg > -1)
        {
            
            //object segement
            pooltype = 1;
            //validate
            if (ls.objseg >= ObjectSegments.Length)
                ls.objseg = 0;
            //get type
            object_type = ls.objseg;
            //spawn
            GameObject obj = object_Pools[ls.objseg].Get();
            obj.transform.parent = lvlseg.transform; //set parent to level segment so we only move 1 thing
            obj.transform.localPosition = Vector3.zero; //reset its local posistion as it is a child now. 
            for (int i = 0; i < obj.transform.childCount;i++)
            {
                obj.transform.GetChild(i).gameObject.SetActive(true);//any game objects attached to it will be activated ( this is from when we hid them on the collision step)
            }
            //set init state of the script. 
            lvlseg.transform.GetComponent<roadtile_Levels>().setspeed( ls.levelseg, ls.objseg, obj);
        }
        else
        {
            //hide if exists
            lvlseg.transform.GetComponent<roadtile_Levels>().setspeed(ls.levelseg,ls.objseg,null);
        }
      
        //ActiveSegments.Add(lvlseg);
        return lvlseg;
    }

    //this one simply spawns 1 at the end of the sequence using the current data. 
  
    public  void spawnnextsegment()
    {
        if (current_order >= levelsmaster[currentlevel].Segments.Count)
        {
            Debug.Log("Level up");
            current_order = 0;



            currentlevel++;
           
            if (currentlevel > levelsmaster.Count - 1)
            {
                currentlevel = 0;
                // levelclear = true;
                //currentsegmentlength = 0;
            }
            speed = levelsmaster[currentlevel].speed;
            levelname.text = levelsmaster[currentlevel].LevelName;
        }
        //currentsegmentlength = levelsmaster[currentlevel].Segments[current_order].segment_length;
        var g = spawnnewsegement(levelsmaster[currentlevel].Segments[current_order], currentsegmentlength);
       
        orderplus();
    }
    //increment and wrap

    private void orderplus ()
    {
        Debug.Log("Order up");
        current_order++;
       
       
    }

   

}

//This is the script we put on the level segement to allow it to control the parts
public class roadtile_Levels : MonoBehaviour
{

    public EndlessRunner_Levels master;

    public bool setactive = false;
    private int id = 0;
    private int _obj;
    GameObject _child;
    public void setspeed(int currentorder, int pool_id, GameObject child)
    {
        // Debug.Log("z pos at start " + this.transform.localPosition.z.ToString());

        id = currentorder;
        setactive = true;
        _obj = pool_id;
        _child = child;
    }
    private void Update()
    {
        if (setactive)
        {
            //move direction * speed * last time taken for last frame. 
            this.transform.Translate(new Vector3(0, 0, -1) * master.speed * Time.deltaTime); ;
            //out of bounds
            if (this.transform.localPosition.z < 0.2f )
            {
                master.pooltype = 1;
                // Debug.Log("z pos at exit "+ this.transform.localPosition.z.ToString()); //this will slow it down use if needed but don't forget to comment out after. 
                if (_obj > -1)//is there a object segment and if so we release it from the pool
                    master.object_Pools[_obj].Release(_child);
                master.spawnnextsegment(); //send command to get next part
                master.pooltype = 0;
                setactive = false; //this goes inactive
                master.level_Pools[id].Release(this.gameObject); //we release the level segment from the pool
            }
        } 
    }
   
    private void LateUpdate()
    {
        
    }
    private void FixedUpdate()
    {

       
    }
    
}

