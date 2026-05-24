using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChartLoader : MonoBehaviour
{
    public TextAsset chartStrAsset;

    public void Start()
    {
        var import = JsonConvert.DeserializeObject<ImportChart>(chartStrAsset.text);
        var bpms = new BpmList(import.bpms.Select(x => (x.beats, x.val)));
        
        ;
    }
}
