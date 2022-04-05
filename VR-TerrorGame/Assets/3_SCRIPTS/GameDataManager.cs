using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    public class GameDataManager : MonoBehaviour
    {

        public InteractiveItem linterna;
        public InteractiveItem llave;

        //public TriggerObjective electricity;
        public JumpscareTrigger lightTrigger;

        public DynamicObject electricitySwither;

        public FileExporter fileExporter;

        float timer = 0.0f;
        float timerLinterna = 0.0f;
        float timerLlave = 0.0f;

        bool keepRunningTimer= false;

        bool hasLinterna;
        bool hasLlave;

        void Start()
        {
            keepRunningTimer = true;
            hasLinterna = false;
            hasLlave = false;
        }

        void Update()
        {
            if (keepRunningTimer)
            {
                timer = timer + Time.deltaTime;
                if (lightTrigger.isPlayed)
                {
                    if (hasLinterna == false)
                    {
                        timerLinterna = timerLinterna + Time.deltaTime;
                    }
                    if (hasLlave == false)
                    {
                        timerLlave = timerLinterna + Time.deltaTime;
                    }
                    if (linterna.itemTaken == true)
                    {
                        hasLinterna = true;
                    }
                    if (llave.itemTaken == true)
                    {
                        hasLlave = true;
                    }
                    if (electricitySwither.isUp)
                    {
                        keepRunningTimer = false;
                        string data = "tiempo_total,tiempo_linterna,tiempo_llave";
                        string time1 = formatTime(timer);
                        string time2 = formatTime(timerLinterna);
                        string time3 = formatTime(timerLlave);
                        string dataTime = time1 + "," + time2 + "," + time3;
                        List<string> list = new List<string>();
                        list.Add(data);
                        list.Add(dataTime);
                        fileExporter.writeCSV("stats.csv", list);
                    }
                }
            }
        }

        string formatTime(float t)
        {
            float minutes = Mathf.Floor(t / 60);
            float seconds = Mathf.Round(t % 60);
            //float seconds = t % 60;
            string minutesString = minutes.ToString();
            string secondsString = seconds.ToString();
            string time = minutesString + ":" + secondsString;
            return time;
        }

    }
}


