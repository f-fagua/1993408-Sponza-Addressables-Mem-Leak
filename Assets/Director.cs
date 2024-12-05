using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class Resource
{
    public AsyncOperationHandle<GameObject> Handle;
    public GameObject Go;
}

public class Director : MonoBehaviour
{
    private Dictionary<string, Resource> _resources = new();

    private AsyncOperationHandle<Data> _dataHandle;
    private AsyncOperation _unloadUnusedOp;
    
    public Material _greenMat;
    public Texture2D _greenTex;
    
    public Material _blueMat;
    public Texture2D _blueTex;
    
    public Data _data;
    
    public Texture2D _dataTex;
    
    private void Update()
    {
        return;
        
        if (_greenMat)
        {
            Debug.LogFormat("Mat {0} {1}", _greenMat, _greenTex);
        }
        else if (_greenTex)
        {
            Debug.LogFormat("Tex {0}", _greenTex);
        }
        
        if (_blueMat)
        {
            Debug.LogFormat("Mat {0} {1}", _blueMat, _blueTex);
        }
        else if (_blueTex)
        {
            Debug.LogFormat("Tex {0}", _blueTex);
        }

        if (_data)
        {
            Debug.LogFormat("Data {0} {1} {2}", _data.Name, _data.Age, _data.Money);
        }
        if (_dataTex)
        {
            Debug.LogFormat("Data Tex {0}", _dataTex);
        }
    }

    public void ChangeScene()
    {
        int idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadSceneAsync((idx + 1) % 2, LoadSceneMode.Single);
    }

    public void Load(string key, Vector3 position)
    {
        if (_resources.ContainsKey(key) == false)
        {
            var resource = new Resource();
            
            resource.Handle = Addressables.InstantiateAsync(key);
            resource.Handle.Completed += handle =>
            {
                resource.Go = handle.Result;
                resource.Go.transform.position = position;
                
                // Hack: keep some refs around to see what happens to them
                if (key == "green")
                {
                    var renderer = resource.Go.GetComponent<Renderer>();
                    _greenMat = renderer.sharedMaterial;
                    _greenTex = (Texture2D)_greenMat.mainTexture;
                }
                else if (key == "blue")
                {
                    var renderer = resource.Go.GetComponent<Renderer>();
                    _blueMat = renderer.material;
                    _blueTex = (Texture2D)_blueMat.mainTexture;
                }
            };
            
            _resources.Add(key, resource);
        }
    }
    
    public void Release(string key)
    {
        if (_resources.TryGetValue(key, out var resource))
        {
            Addressables.Release(resource.Handle);
            if (resource.Go == false)
            {
                _resources.Remove(key);
            }
        }
    }
    
    public void DestroyWithKey(string key)
    {
        if (_resources.TryGetValue(key, out var resource))
        {
            if (resource.Handle.IsValid() == false)
            {
                Destroy(resource.Go);
                _resources.Remove(key);
            }
            else if (resource.Handle.Status == AsyncOperationStatus.Succeeded)
            {
                Destroy(resource.Go);
            }
            else
            {
                resource.Handle.Completed += handle =>
                {
                    Destroy(handle.Result);
                };
            }
        }
    }
    
    public void DestroyAndReleaseWithKey(string key)
    {
        if (_resources.TryGetValue(key, out var resource))
        {
            if (resource.Handle.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(resource.Handle);
                Destroy(resource.Go);
                _resources.Remove(key);
            }
            else
            {
                resource.Handle.Completed += handle =>
                {
                    Destroy(handle.Result);
                    Addressables.Release(handle);
                    _resources.Remove(key);
                };
            }
        }
    }
    
    public void UnloadUnusedResources()
    {
        _unloadUnusedOp = Resources.UnloadUnusedAssets();
        _unloadUnusedOp.completed += operation =>
        {
            Debug.Log("Unloaded Unused Assets Completed!");
        };
    }
    
    public void UnloadAllBundles()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }
    
    public void LogLoadedBundles()
    {
        StringBuilder sb = new StringBuilder();
        var bundles = AssetBundle.GetAllLoadedAssetBundles();

        sb.AppendLine("Currently Loaded Bundles");
        foreach (var bundle in bundles)
        {
            sb.AppendFormat("\n - {0}", bundle.name);
        }
        Debug.Log(sb.ToString());
    }

    public void LoseDataRef()
    {
        _data = null;
    }
    
    public void LoadData()
    {
        if (_dataHandle.IsValid() == false)
        {
            _dataHandle = Addressables.LoadAssetAsync<Data>("Data");
            _dataHandle.Completed += op =>
            {
                _data = op.Result;
                _dataTex = _data.Tex;
            };
        }
    }
    
    public void UnloadData()
    {
        if (_dataHandle.IsValid())
        {
            Addressables.Release(_dataHandle);
        }
    }
}
