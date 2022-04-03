using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    public class GameDataManager : MonoBehaviour
    {

        public InteractiveItem linterna;
        public InteractiveItem llave;

        public JumpscareTrigger lightTrigger;
        public Electricity electricity;

        public FileExporter fileExporter;

        float timer;
        float timerLinterna;
        float timerLlave;

        private bool keepRunningTimer= false;

        private bool hasLinterna = false;
        private bool hasLlave = false;

        void Start()
        {
            keepRunningTimer = true; 
        }

        void Update()
        {
            if(keepRunningTimer)
            {
                timer = timer+Time.deltaTime;
                if(lightTrigger.isPlayed)
                {
                    if (linterna.itemTaken == true)
                    {
                        hasLinterna = true;
                    }
                    if (llave.itemTaken == true)
                    {
                        hasLlave = true;
                    }
                    if (!hasLinterna)
                    {
                        timerLinterna = timerLinterna + Time.deltaTime;
                    }
                    if(!hasLlave)
                    {
                        timerLlave = timerLinterna + Time.deltaTime;
                    }

                    if(electricity.isPoweredOn)
                    {
                        
                        keepRunningTimer =false;
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
            //float seconds = Mathf.Floor(t % 60);
            float seconds = t % 60;
            string minutesString = minutes.ToString();
            string secondsString = seconds.ToString();
            string time = minutesString + ":" + secondsString;
            return time;
        }

    }
}


