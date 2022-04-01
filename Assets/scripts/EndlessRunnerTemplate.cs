using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class roadtile : MonoBehaviour
{
    public EndlessRunnerTemplate master;
    public int speed=10;
    public bool setactive = false;
    int id = 0;
    public void setspeed (int _speed,int _id)
    {
       // Debug.Log("z pos at start " + this.transform.localPosition.z.ToString());
        speed = _speed;
        id = _id;
        setactive = true;
        
    }
  
    private void FixedUpdate()
    {
        
    if (setactive)
        { 
            this.transform.Translate(new Vector3(0, 0, -1) * speed * Time.deltaTime );
            if (this.transform.localPosition.z < 0.2f)
            {
                 Debug.Log("z pos at exit "+ this.transform.localPosition.z.ToString());
       
                //master.ActiveSegments.RemoveAt(id);
                master.spawnnextsegment();
                setactive = false;
                PoolManager.ReleaseObject(this.gameObject);
                //Destroy(this.gameObject);
            }
        }
    }
}
public class EndlessRunnerTemplate : MonoBehaviour
{
    public Transform contentholder;
    public Transform dynamicholder;
    public GameObject[] LevelSegments;
    public GameObject[] ObjectSegments;
    public bool autostart = true;
    public int[] levelorder;
    public int[] objectsegmentorder;
     public int speed = 10;
     public int segment_length = 100;
     public int seg_count = 1;
    public int time = -10;
    bool ready = false;
    public List<GameObject> ActiveSegments = new List<GameObject>();
     List<ulong> SegmentLevelPools = new List<ulong>();
     List<ulong> SegmentObjectPools = new List<ulong>();
     int levelordercount=0;
     int current_order = 0;
    private void Awake()
    {
        levelordercount = levelorder.Length;
        foreach (var g in LevelSegments)
        {
            roadtile rt = g.AddComponent<roadtile>();
            rt.speed = speed;
            rt.master = this;

            rt.setactive = false;
            SegmentLevelPools.Add(PoolManager.WarmPoolu(g, 2, dynamicholder));
        }
        foreach (var g in ObjectSegments)
            SegmentObjectPools.Add(PoolManager.WarmPoolu(g, 2, dynamicholder));

    }
    // Start is called before the first frame update

    private void FixedUpdate()
    {
        time++;
        if (!ready)
            if (autostart & time > -1)
            {
                for (int x = 0; x < seg_count; x++)
                {
                    spawnnewsegement(levelorder[current_order], objectsegmentorder[current_order], segment_length * (x + 1));
                    current_order++;
                    if (current_order >= levelordercount)
                        current_order = 0;
                }


                ready = true;
            }
    }

     GameObject spawnnewsegement(int seg, int obj, float dis)
    {
        var lvlseg = PoolManager.SpawnObject(SegmentLevelPools[seg], contentholder, new Vector3(0, 0, dis));
        //var lvlseg = Instantiate(LevelSegments[seg],  contentholder,false);
        lvlseg.transform.localPosition= new Vector3(0, 0, dis);


        if (obj > -1)
        {
            var lvlobj = PoolManager.SpawnObject(SegmentObjectPools[obj], lvlseg.transform, Vector3.zero);
           // var lvlobj =  Instantiate(ObjectSegments[obj], lvlseg.transform,false);
            lvlobj.transform.localPosition = Vector3.zero;
        }
        lvlseg.transform.GetComponent<roadtile>().setspeed((int)Mathf.Clamp(speed, 10.0f, 1000.0f),ActiveSegments.Count);
        //ActiveSegments.Add(lvlseg);
        return lvlseg;
    }
    public  void spawnnextsegment()
    {
       
        var g = spawnnewsegement(levelorder[current_order], objectsegmentorder[current_order], segment_length*seg_count);
        current_order++;
        if (current_order >= levelordercount)
            current_order = 0;
    }
   
  
    
}
