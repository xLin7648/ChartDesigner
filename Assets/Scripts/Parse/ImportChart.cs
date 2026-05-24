using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportChart
{
    public float offset;
    public List<BpmItem> bpms;

    public class BpmItem
    {
        public float beats;
        public float val;
    }
}