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
        public DynamicObject electricitySwither;
        public FileExporter fileExporter;

        // TIEMPOS 
        float timer = 0.0f;
        float timerExploracion = 0.0f;
        float timerReaccion = 0.0f;
        bool keepRunningTimer = false;

        float timerLinterna = 0.0f;
        float timerLlave = 0.0f;

        
        float timerSalon = 0.0f;
        bool keepRunningTimerSalon = false;
        int flagSalon = 0;

        float timerComedor = 0.0f;
        bool keepRunningTimerComedor = false;
        int flagComedor = 0;

        float timerBano = 0.0f;
        bool keepRunningTimerBano = false;
        int flagBano = 0;

        float timerCocina = 0.0f;
        bool keepRunningTimerCocina = false;
        int flagCocina = 0;

        float timerDormitorio = 0.0f;
        bool keepRunningTimerDormitorio = false;
        int flagDormitorio = 0;
        int flagDormitorio2 = 0;

        bool hasLinterna;
        bool hasLlave;

        int cont1 = 0;
        int cont2 = 0;

        void Start()
        {
            keepRunningTimer = true;
            hasLinterna = false;
            hasLlave = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "MISION" && flagSalon != 1)
            {
                keepRunningTimerSalon = true;
                flagSalon =  1;
                Debug.Log("BUSCA SALON");
            }
            if (other.tag == "SALON" && flagComedor != 1)
            {
                keepRunningTimerSalon = false;
                keepRunningTimerComedor = true;
                flagComedor = 1;
                Debug.Log("BUSCA COMEDOR");
            }
            if (other.tag == "COMEDOR" && flagBano != 1)
            {
                keepRunningTimerComedor = false;
                keepRunningTimerBano = true;
                flagBano = 1;
                Debug.Log("BUSCA BAÑO");
            }
            if (other.tag == "BANO" && flagCocina !=1)
            {
                keepRunningTimerBano=false;
                keepRunningTimerCocina=true;
                flagCocina = 1;
                Debug.Log("BUSCA COCINA");
            }
            if (other.tag == "COCINA" && flagDormitorio !=1)
            {
                keepRunningTimerCocina=false;
                keepRunningTimerDormitorio = true;
                flagDormitorio=1;
                Debug.Log("BUSCA DORMITORIO");
            }
            if (other.tag == "DORMITORIO" && flagDormitorio2 !=1)
            {
                keepRunningTimerDormitorio=false;
                timerExploracion = timer;
                flagDormitorio2 = 1;
                Debug.Log("LUCES");
            }
        }

        void Update()
        {
            if (keepRunningTimer)
            {
                timer = timer + Time.deltaTime;

                if(keepRunningTimerSalon)
                {
                    timerSalon = timerSalon + Time.deltaTime;
                }
                if (keepRunningTimerComedor)
                {
                    timerComedor = timerComedor + Time.deltaTime;
                }
                if (keepRunningTimerBano)
                {
                    timerBano = timerBano + Time.deltaTime;
                }
                if (keepRunningTimerCocina)
                {
                    timerCocina = timerCocina + Time.deltaTime;
                }
                if (keepRunningTimerDormitorio)
                {
                    timerDormitorio = timerDormitorio + Time.deltaTime;
                }

                if (lightTrigger.isPlayed)
                {
                    if (hasLinterna == false)
                    {
                        timerLinterna = timerLinterna + Time.deltaTime;
                    }
                    if (hasLlave == false)
                    {
                        timerLlave = timerLlave + Time.deltaTime;
                    }
                    if (linterna.itemTaken == true && cont1 != 1)
                    {
                        hasLinterna = true;
                        cont1 = 1;
                        Debug.Log("TIENE LINTERNA");
                        Debug.Log("Timepo linterna = "+timerLinterna);
                    }
                    if (llave.itemTaken == true && cont2 != 1)
                    {
                        hasLlave = true;
                        cont2 = 1;
                        Debug.Log("TIENE LLAVE");
                        Debug.Log("Timepo llave = " + timerLlave);
                    }
                    if (electricitySwither.isUp)
                    {
                        keepRunningTimer = false;

                        List<string> list = new List<string>();

                        string datosHabitaciones = "t_salon;t_comedor;t_aseo;t_cocina;t_dormitorio";
                        /*
                        string timeSalon = formatTime(timerSalon);
                        string timeComedor = formatTime(timerComedor);
                        string timeBano = formatTime(timerBano);
                        string timeCocina = formatTime(timerCocina);
                        string timeDormitorio = formatTime(timerDormitorio);
                        */
                        string timeSalon = timerSalon.ToString();
                        string timeComedor = timerComedor.ToString();
                        string timeBano = timerBano.ToString();
                        string timeCocina = timerCocina.ToString();
                        string timeDormitorio = timerDormitorio.ToString();
                        string timeHabitaciones = timeSalon + ";" + timeComedor + ";" + timeBano + ";" + timeCocina + ";" + timeDormitorio;

                        list.Add(datosHabitaciones);
                        list.Add(timeHabitaciones);


                        string datosExploracion = "tiempo_exploracion";
                        //string timeExploracion = formatTime(timerExploracion);
                        string timeExploracion = timerExploracion.ToString();

                        list.Add(datosExploracion);
                        list.Add(timeExploracion);

                        string datosObjetos = "t_linterna;t_llave";
                        /*
                        string timeLinterna = formatTime(timerLinterna);
                        string timeLlave = formatTime(timerLlave);
                        */
                        string timeLinterna = timerLinterna.ToString(); 
                        string timeLlave = timerLlave.ToString(); 

                        string timeObjetos = timeLinterna + "," + timeLlave;

                        list.Add(datosObjetos);
                        list.Add(timeObjetos);

                        string datosReaccion = "tiempo_reaccion";
                        timerReaccion = timer - timerExploracion;
                        // string timeReaccion = formatTime(timerReaccion);
                        string timeReaccion = timerReaccion.ToString();

                        list.Add(datosReaccion);
                        list.Add(timeReaccion);

                        string datosTotal = "tiempo_total";
                        //string timeTotal = formatTime(timer);
                        string timeTotal = timer.ToString();

                        list.Add(datosTotal);
                        list.Add(timeTotal);

                        Debug.Log("DATOS REGISTRADOS");
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


