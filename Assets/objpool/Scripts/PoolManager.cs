using System;
using System.Collections.Generic;
using MonsterLove.Collections;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{

 
    public bool logStatus;
	//public Transform root;
    private ulong pmoid = 0;
    private Dictionary<ulong, GameObject> instances;
	private Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup;
	private Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup;
    private Dictionary<GameObject, ulong> poolobjectmap;
	private bool dirty = false;
	
	void Awake () 
	{
		prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
		instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
        poolobjectmap = new Dictionary<GameObject, ulong>();
        activetracking = new Dictionary<ulong, GameObject>();
        instances = new Dictionary<ulong, GameObject>();
	}

	void Update()
	{
		if(logStatus && dirty)
		{
			PrintStatus();
			dirty = false;
		}
	}
    public ulong WarmPool(GameObject prefab, int size,Transform parent)
    {
        if (poolobjectmap.ContainsKey(prefab))
        {
            prefabLookup[prefab].AddSize(size);
            return poolobjectmap[prefab];
            //throw new Exception("Pool for prefab " + prefab.name + " has already been created");
        }
        pmoid++;
        poolobjectmap[prefab]= pmoid;
        instances[pmoid] = prefab;
        var pool = new ObjectPool<GameObject>(() => { return InstantiatePrefab(prefab,parent); }, size);
        prefabLookup[prefab] = pool;
        dirty = true;
        return pmoid;
    }
    public GameObject spawnObject(ulong id)
    {
        return spawnObject(instances[id], Vector3.zero, Quaternion.identity);
    }
    public GameObject spawnObject(ulong id,Vector3 pos)
    {
        return spawnObject(instances[id], pos, Quaternion.identity);
    }
    public GameObject spawnObject(ulong id,Transform parent)
    {
       
        return spawnObject(instances[id], Vector3.zero, Quaternion.identity, parent);
    }
    public GameObject spawnObject(ulong id,Vector3 pos, Transform parent)
    {

        return spawnObject(instances[id], pos, Quaternion.identity, parent);
    }
    public GameObject spawnObject(GameObject prefab)
	{
		return spawnObject(prefab, Vector3.zero, Quaternion.identity);
	}

	public GameObject spawnObject(GameObject prefab, Vector3 position, Quaternion rotation,Transform parent = null)
	{

		if (!prefabLookup.ContainsKey(prefab))
		{
			WarmPool(prefab, 1,parent);
		}

		var pool = prefabLookup[prefab];

		var clone = pool.GetItem();
       
	
      
        if (!(parent is null))
        {
            //LogManager.Log("parent found.");
            clone.transform.SetParent(parent,false);
           
        }
        else
            Debug.Log("parent not found.");
       	clone.transform.localPosition = position;
		clone.transform.rotation = rotation;
         clone.SetActive(true);
		instanceLookup.Add(clone, pool);
		dirty = true;
        
		return clone;
	}

	public void releaseObject(GameObject clone)
	{
		clone.SetActive(false);

		if(instanceLookup.ContainsKey(clone))
		{
			instanceLookup[clone].ReleaseItem(clone);
			instanceLookup.Remove(clone);
			dirty = true;
		}
		else
		{
			Debug.LogWarning("No pool contains the object: " + clone.name);
		}
	}


	private GameObject InstantiatePrefab(GameObject prefab,Transform parent)
	{
		var go = Instantiate(prefab,parent,false) as GameObject;
		//if (parent == null && root != null) go.transform.parent = root;
		return go;
	}

	public void PrintStatus()
	{
		foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyVal in prefabLookup)
		{
			Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key.name, keyVal.Value.CountUsedItems, keyVal.Value.Count));
		}
	}

    public  void clearinstances()
    {
        foreach (var ob in instanceLookup)
        {
            instanceLookup[ob.Key].ReleaseItem(ob.Key);
            instanceLookup.Remove(ob.Key);
          
        }
      
        dirty = true;
    }
    public  void clearprefabs()
    {
        foreach (var ob in prefabLookup)
        {
            prefabLookup[ob.Key].ReleaseItem(ob.Key);
            prefabLookup.Remove(ob.Key);

        }
        dirty = true;
    }

	#region Static API
    public static void clearallinstances()
    {
        Instance.clearinstances();
    }
    public static void clearallprefabs()
    {
        Instance.clearprefabs();
    }
    public static ulong WarmPoolu(GameObject prefab, int size,Transform parent)
	{
		return Instance.WarmPool(prefab, size,parent);
	}

    static Dictionary<ulong, GameObject> activetracking;
    static ulong pmid = 0;
    public static ulong SpawnObject(ulong id)
    {

        activetracking[pmid] = Instance.spawnObject(id);
        ulong lid = pmid;
        pmid++;
        return lid;
    }
    public static ulong SpawnObject(ulong id,Transform parent)
    {

        activetracking[pmid] = Instance.spawnObject(id,parent);
        ulong lid = pmid;
        pmid++;
        return lid;
    }
    public static ulong SpawnObject(ulong id, Vector3 pos, Transform parent)
    {

        activetracking[pmid] = Instance.spawnObject(id, pos,parent);
        ulong lid = pmid;
        pmid++;
        return lid;
    }
    public static GameObject SpawnObject(ulong id,  Transform parent, Vector3 pos)
    {
        GameObject result = Instance.spawnObject(id, pos, parent);
        activetracking[pmid] = result;      
        pmid++;
        return result;
    }
    public static GameObject SpawnObjectg(ulong id, Transform parent)
    {
        GameObject result = Instance.spawnObject(id,  parent);
        activetracking[pmid] = result;
        pmid++;
        return result;
    }
    public static ulong SpawnObject(ulong id, Vector3 pos)
    {

        activetracking[pmid] = Instance.spawnObject(id, pos);
        ulong lid = pmid;
        pmid++;
        return lid;
    }
    public static ulong SpawnObject(GameObject prefab)
	{

        activetracking[pmid] =Instance.spawnObject(prefab);
        ulong id = pmid;
        pmid++;
        return id;
	}
    public static ulong SpawnObject(GameObject prefab,Transform parent)
    {
        GameObject result = Instance.spawnObject(prefab);
        result.transform.SetParent(parent, false);
        activetracking[pmid] = result;
        ulong id = pmid;
        pmid++;
        return id;
    }

    public static ulong SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		
        activetracking[pmid] = Instance.spawnObject(prefab, position, rotation);
        ulong id = pmid;
        pmid++;
        return id;
    }
    public static GameObject GetObject(ulong id)
    {
        if (activetracking.ContainsKey(id))
            return activetracking[id];
        else return null;
    }
	public static void ReleaseObject(ulong id)
	{
		Instance.releaseObject(activetracking[id]);
	}
    public static void ReleaseObject(GameObject g)
    {
        Instance.releaseObject(g);
    }

    #endregion
}


