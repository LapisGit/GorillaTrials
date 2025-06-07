namespace GorillaTrials
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class CustomProps
    {
        [JsonProperty("PBs")]
        public List<Dictionary<string, float>> PBs { get; set; } = new List<Dictionary<string, float>>();
        public void AddPB(string trialName, float time)
        {
            PBs.Add(new Dictionary<string, float> { { trialName, time } });
        }
    }
}