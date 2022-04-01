using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

//simple holder for each segement to let us know what level and what object. 
[System.Serializable]
public class levelsegment
{
    public int levelseg;
    public int objseg;
}
public class EndlessRunner_UnityPool : MonoBehaviour
{
    public TMPro.TMP_Text timetext;
   
    public Transform contentholder; //where we spawn level segments
    public GameObject[] LevelSegments; //prefabs of level segments
    public GameObject[] ObjectSegments; //prefabs of object segments
    public bool autostart = true; //do we start at time = 0
    public int speed = 50; //speed of trackmovement
   
    public int segment_length = 15; //z length to tile the segements
    public int seg_count = 6; //how many to make in front of player
    public int time = -10; //updates ++, number of frames played
    public levelsegment[] levelorder;  //class at bottom holding int int level id and object id

    private bool ready = false;  //make sure stuff is ready before we start internal 
    private int current_order = 0; //which levelorder index are we using, use length of it to trigger cycle start over at 0

    //unity pooling
    public bool collectionChecks = true;
    public int maxPoolSize = 10;
    public List<IObjectPool<GameObject>> level_Pools;
    public List<IObjectPool<GameObject>> object_Pools;
    private int type = 0; //0 = level pool 1 = object pool, I share the same pooling system with multiple types
    private int level_type = 0; //which prefab to use
    private int object_type = 0;//which prefab to use
  
    private void Awake()
    {
      //init
        level_Pools = new List<IObjectPool<GameObject>>();
        object_Pools = new List<IObjectPool<GameObject>>();

        //level seg type = 0, loading the prefabs into the pools. 
        type = 0;
        foreach (var g in LevelSegments)
        {
            //add the road tile script for movement of the world
            roadtile_UP rt = g.AddComponent<roadtile_UP>();         
            rt.master = this; //link this script to this master so we can commuinicate back. 
            rt.setactive = false;          //don't move yet
            level_Pools.Add( new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize));
            level_type++;
        }
        //object seg type = 0, loading the prefabs into the pools. 
        type = 1;
        foreach (var g in ObjectSegments)
        { 
            object_Pools.Add(new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize));
           
            object_type++;
        }


    }
    //happens every physics step, you can control in time in project settings
    private void FixedUpdate()
    {
        time++;//we increment time
        timetext.text = "Time: " + time.ToString();
       
        if (!ready) //starts off
            if (autostart & time > -1) //time == 0 and we want to autostart
            {
                for (int x = 0; x < seg_count; x++) //for each active segement
                {
                    spawnnewsegement(levelorder[current_order], segment_length * (x + 1)); //create a new active segement for both level and object
                    orderplus(); //increment the current_order but check to see if we need to loop it back to 0
                }


                ready = true; // now it is true we will not repeat this step. 
            }
      
    }
   
    //required by object pooler, I use type here to make what object I need, and level / object type to get the segment. 
    //so I was able to reuse the pooler instead of make a new one per item. Just have 2 types supported now but more could be added. 
    GameObject CreatePooledItem()
    {

      
        if (type == 0)           
          return   Instantiate(LevelSegments[level_type], contentholder, false);
        else 
          return  Instantiate(ObjectSegments[object_type], contentholder, false);
       
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(GameObject system)
    {
        system.SetActive(false);
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(GameObject system)
    {
        system.SetActive(true);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(GameObject system)
    {
        Destroy(system);
    }
  
  
    //this is the reusable function to make a new level segment at x dis
     GameObject spawnnewsegement(levelsegment ls, float dis)
    {
        //spawn the level
        type = 0;
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
            type = 1;
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
            lvlseg.transform.GetComponent<roadtile_UP>().setspeed( ls.levelseg, ls.objseg, obj);
        }
        else
        {
            //hide if exists
            lvlseg.transform.GetComponent<roadtile_UP>().setspeed(ls.levelseg,ls.objseg,null);
        }
      
        //ActiveSegments.Add(lvlseg);
        return lvlseg;
    }

    //this one simply spawns 1 at the end of the sequence using the current data. 
    public  void spawnnextsegment()
    {
       
        var g = spawnnewsegement(levelorder[current_order], segment_length*seg_count);
        orderplus();
    }
    //increment and wrap
    private void orderplus ()
    {
        current_order++;
        if (current_order >= levelorder.Length)
            current_order = 0;
    }

   

}

//This is the script we put on the level segement to allow it to control the parts
class roadtile_UP : MonoBehaviour
{

    public EndlessRunner_UnityPool master;

    public bool setactive = false;
    private int id = 0;
    private int _obj;
    GameObject _child;
    public void setspeed(int currentorder, int obj, GameObject child)
    {
        // Debug.Log("z pos at start " + this.transform.localPosition.z.ToString());

        id = currentorder;
        setactive = true;
        _obj = obj;
        _child = child;
    }
    private void Update()
    {
        if (setactive)
        {
            //move direction * speed * last time taken for last frame. 
            this.transform.Translate(new Vector3(0, 0, -1) * master.speed * Time.deltaTime); ;
            //out of bounds
            if (this.transform.localPosition.z < 0.2f)
            {
                // Debug.Log("z pos at exit "+ this.transform.localPosition.z.ToString()); //this will slow it down use if needed but don't forget to comment out after. 
                if (_obj > -1)//is there a object segment and if so we release it from the pool
                    master.object_Pools[_obj].Release(_child);
                master.spawnnextsegment(); //send command to get next part
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

