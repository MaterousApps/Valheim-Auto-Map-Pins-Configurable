using AMP_Configurable.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Utilities;


namespace AMP_Configurable.Commands
{
    public static class AMP_Commands
    {
        public static int pageSize = 11;
        private static Vector3 chatPos = new Vector3(0.0f, -99f);
        private static ModConsoleOpt consoleOpt = null;
        private static List<string> filterList = new List<string>()
        {
          "Berries",
          "Blueberries",
          "Carrot",
          "Cloudberries",
          "Copper",
          "Crypt",
          "DragonEgg",
          "Draugr",
          "Flametal",
          "Greydwarf",
          "Iron",
          "Leviathan",
          "Mushroom",
          "Obsidian",
          "Serpent",
          "Silver",
          "Skelton",
          "SunkenCrypt",
          "Surtling",
          "Thistle",
          "Tin",
          "TrollCave",
          "Turnip"
        };


        public static Dictionary<string, string> commandList = new Dictionary<string, string>()
        {
            {
            "/AMP-Clear",
            "[PIN] - Clear the selected type of saved pins. No parameter provided will return a list of available pins. -1 will clear all saved pins."
            },
            {
            "/AMP-ClearAll",
            "- Clear all pins created by the AMP mod (including temporary ones around you)."
            },
            {
            "/AMP-Filter",
            "[PIN] - Filter the selected Pin. No parameter provided will list all pins. -1 will show all pins."
            },
            {
            "/AMP-FilterList",
            "- Displays the list of currently filtered pins."
            }
        };

        internal static ModConsoleOpt ConsoleOpt
        {
            get => consoleOpt;
            set => consoleOpt = value;
        }

        public static void Announce()
        {
            try
            {
                commandList = commandList.OrderBy(obj => obj.Key).ToDictionary((obj => obj.Key), (obj => obj.Value));
                filterList.Sort();
                Patches.AMPCommandPatcher.InitPatch();
            }
            catch (Exception ex)
            {
            }
        }

        public static void ProcessCommands(
          string inCommand,
          GameObject go = null)
        {
            if (string.IsNullOrEmpty(inCommand))
                return;
            if (Console.instance != null)
                Console.instance.SetPrivateField("m_lastEntry", inCommand);
            string[] strArray = inCommand.Split(';');
            foreach (string str in strArray)
            {
                string inCommand1 = str.Trim();
                if (!ProcessCommand(inCommand1, go))
                {
                    if (strArray.Length > 1)
                    {
                        Console.instance.m_input.text = inCommand1;
                        Console.instance.InvokePrivateMethod("InputText", null);
                    }
                    Console.instance.m_input.text = string.Empty;
                }
            }
        }

        public static bool ProcessCommand(
          string inCommand,
          GameObject go = null)
        {
            if (string.IsNullOrEmpty(inCommand) || string.IsNullOrWhiteSpace(inCommand))
                return true;
            inCommand = inCommand.Trim();
            string[] strArray1 = inCommand.Split(' ');
            if (inCommand.StartsWith("help"))
            {
                Console.instance.Print("/AMP? [Page] - AMP Commands - Ex /AMP? 1");
                return false;
            }
            if (strArray1[0].Equals("/AMP?"))
            {
                int result = 1;
                if (strArray1.Length > 1 && int.TryParse(strArray1[1], out result))
                {
                    if (result > Mathf.Ceil((commandList.Count / pageSize)) + (commandList.Count % pageSize == 0 ? 0.0 : 1.0))
                        result = Mathf.RoundToInt(Mathf.Ceil((float)(commandList.Count / pageSize)) + (commandList.Count % pageSize == 0 ? 0.0f : 1f));
                    List<string> stringList1 = new List<string>(commandList.Keys);
                    List<string> stringList2 = new List<string>(commandList.Values);
                    PrintOut("Command List Page " + result + " / " + (float)(Mathf.Ceil((commandList.Count / pageSize)) + (commandList.Count % pageSize == 0 ? 0.0 : 1.0)));
                    for (int index = pageSize * result - pageSize; index < (commandList.Count > pageSize * (result + 1) - pageSize ? pageSize * (result + 1) - pageSize : commandList.Count); ++index)
                        PrintOut(stringList1[index] + " " + stringList2[index]);
                }
                else
                    PrintOut("Type /AMP? # to see the help for that page number. Ex. /? 1");
                return true;
            }
            if (commandList.ContainsKey(strArray1[0]) && Player.m_localPlayer == null)
            {
                PrintOut("Where are you trying to run these commands? We can't process them in the real world!");
                return true;
            }

            if (strArray1[0].Equals("/AMP-Clear"))
            {
                if (strArray1.Length == 1)
                {
                    foreach (string text in filterList.OrderBy((q => q)).ToList())
                        PrintOut(text);
                }
                else if (strArray1.Length >= 2)
                {
                    //display all pins
                    if (strArray1[1].Equals("-1"))
                    {
                        foreach (Minimap.PinData pins in Mod.savedPins)
                        {
                            if ((int)pins.m_type >= 100 && pins.m_save)
                            {
                                Minimap.instance.RemovePin(pins);
                            }
                        }
                        PrintOut("Cleared all saved pins.");
                    }
                    else
                    {
                        string str1 = string.Empty;
                        string str2;
                        if (strArray1.Length > 2)
                        {
                            foreach (string str3 in strArray1)
                            {
                                if (!str3.Equals(strArray1[0]))
                                    str1 = str1 + " " + str3;
                            }
                            str2 = str1.Trim();
                        }
                        else
                            str2 = strArray1[1];

                        if (filterList.Contains(str2))
                        {
                            PinnedObject.loadData(null, str2);

                            foreach (Minimap.PinData pins in Mod.savedPins)
                            {
                                if ((int)pins.m_type >= 100 && pins.m_save && (int)pins.m_type == PinnedObject.pType)
                                {
                                    Minimap.instance.RemovePin(pins);
                                }
                            }
                            PrintOut("Cleared " + str2 + " pins");
                        }
                        else
                            PrintOut("Failed to clear pins '" + str2 + "'. Check parameters! Ex. /AMP-Clear, /AMP-Clear -1, /AMP-Clear Berries");
                    }
                }
                else
                    PrintOut("Failed to filter pins. Check parameters! Ex. /AMP-Clear, /AMP-Clear -1, /AMP-Clear Berries");
                return true;
            }

            if (strArray1[0].Equals("/AMP-ClearAll"))
            {
                foreach (Minimap.PinData pins in Mod.savedPins)
                {
                    if ((int)pins.m_type >= 100)
                    {
                        Minimap.instance.RemovePin(pins);
                    }
                }
                PrintOut("Cleared all AMP pins, included temporary ones near the player.");
                return true;
            }

            if (strArray1[0].Equals("/AMP-Filter"))
            {
                if (strArray1.Length == 1)
                {
                    foreach (string text in filterList.OrderBy((q => q)).ToList())
                        PrintOut(text);
                }
                else if (strArray1.Length >= 2)
                {
                    //display all pins
                    if (strArray1[1].Equals("-1"))
                    {
                        foreach (Minimap.PinData pins in Mod.autoPins)
                        {
                            pins.m_uiElement.gameObject.SetActive(true);
                        }
                        Mod.filteredPins.Clear();
                        PrintOut("Showing all pins");
                    }
                    else
                    {
                        string str1 = string.Empty;
                        string str2;
                        if (strArray1.Length > 2)
                        {
                            foreach (string str3 in strArray1)
                            {
                                if (!str3.Equals(strArray1[0]))
                                    str1 = str1 + " " + str3;
                            }
                            str2 = str1.Trim();
                        }
                        else
                            str2 = strArray1[1];

                        if (filterList.Contains(str2))
                        {
                            PinnedObject.loadData(null, str2);

                            if (Mod.filteredPins.Contains(PinnedObject.pType.ToString()))
                            {
                                PrintOut("Showing Filtered Pin: " + str2);
                                Mod.filteredPins.Remove(PinnedObject.pType.ToString());
                            }
                            else
                            {
                                Mod.filteredPins.Add(PinnedObject.pType.ToString());
                                PrintOut("Filtering Pins: " + str2);
                            }
                        }
                        else
                            PrintOut("Failed to filter pins '" + str2 + "'. Check parameters! Ex. /AMP-Filter, /AMP-Filter -1, /AMP-Filter Berries");
                    }
                }
                else
                    PrintOut("Failed to filter pins. Check parameters! Ex. /AMP-Filter, /AMP-Filter -1, /AMP-Filter Berries");
                return true;
            }

            if (strArray1[0].Equals("/AMP-FilterList"))
            {
                string output = "";

                for (int x = 0; x < Mod.filteredPins.Count(); x++)
                {
                    PinnedObject.loadData(null, Mod.filteredPins[x]);


                    if (output == null || output == "")
                        output = PinnedObject.aName;
                    else
                        output += ", " + PinnedObject.aName;
                }

                PrintOut("Currently filtering pins: " + output);
                return true;
            }

            return true;
        }

        public static void PrintOut(string text)
        {
            if (text.Equals(string.Empty) || text.Equals(" "))
                return;
            if (Console.instance != null)
            {
                Console.instance.Print("[AMP Commands] " + text);
            }
        }
    }

    public class AMPLoader : MonoBehaviour
    {
        private static GameObject _AMPGameObject;
        private static bool FirstLoad = true;
        private static bool InitLogging = false;
        private static AMPModuleController AMPModuleController;

        public static void Unload()
        {
            Destroy(_AMPGameObject, 0.0f);
            _AMPGameObject = null;
        }

        public static void Reload()
        {
            Unload();
            Init();
        }

        public static GameObject Load
        {
            get => _AMPGameObject;
            set => _AMPGameObject = value;
        }

        private void Start() => Init();

        public static void Main(string[] args) => InitThreading();

        public static void InitThreading() => new Thread(() =>
        {
            Thread.Sleep(5000);
            Init();
        }).Start();

        public static void InitWithLog()
        {
            InitLogging = true;
            Init();
        }

        public static void Init()
        {
            _AMPGameObject = new GameObject("AMP Commands");
            if (InitLogging)
                InitLogging = false;

            if (FirstLoad)
                ResourceUtils.Logz(new string[1]
                {
                  "Commands"
                }, new string[1] { "SUCCESS!" });

            CheckForUnknownInstance();
            Load.transform.parent = null;
            Transform parent = Load.transform.parent;
            if (parent != null && parent.gameObject != Load)
                parent.parent = Load.transform;
            _AMPGameObject.AddComponent<AMPModuleController>();
            DontDestroyOnLoad(_AMPGameObject);
            FirstLoad = false;
        }

        public static void CheckForUnknownInstance()
        {
            foreach (GameObject gameObject in ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).Where(obj => obj.name == "AMP Commands"))
            {
                if (gameObject != _AMPGameObject)
                {
                    Destroy(gameObject);

                }
            }
        }

        private void OnDestroy() => Unload();
    }

    public class AMPModuleController : MonoBehaviour
    {
        private static Version AMPMainVersion = new Version(1, 1, 3);
        internal ResourceUtils.Status AMPMainStatus;
        private bool FirstLoad = true;
        private bool NeedLoadModules = true;
        private bool NeedRetry;
        private bool ErrorMonitor;
        private int RetryCount = 1;
        private int RetryCountMax = 3;
        private ModConsoleOpt moduleConsole;

        private List<AMPBaseModule> MenuOptions { get; set; } = new List<AMPBaseModule>();

        private List<AMPBaseModule> RetryModule { get; set; } = new List<AMPBaseModule>();

        public void Start()
        {
            AMPMainStatus = ResourceUtils.Status.Loading;
            //ResourceUtils.Logz(new string[2]
            //{
            //    "AMP",
            //    "NOTIFY"
            //}, new string[2] { "LOADING...", "MODULE LOADING..." });
            BeginMainMenu();

            Init();
        }

        public void Update()
        {
            if (!FirstLoad)
            {
                if (AMPMainStatus == ResourceUtils.Status.Loading && NeedLoadModules && !NeedRetry)
                {
                    foreach (AMPBaseModule menuOption in MenuOptions)
                    {
                        //ResourceUtils.Logz(new string[3]
                        //{
                        //    "AMP",
                        //    "MODULE",
                        //    "NOTIFY"
                        //}, new string[2]
                        //{
                        //    "NAME: " + menuOption.ModuleName.ToUpper(),
                        //    "STATUS: " + menuOption.ModuleStatus.ToString().ToUpper()
                        //});
                        if (menuOption.ModuleStatus != ResourceUtils.Status.Ready)
                        {
                            NeedRetry = true;
                            RetryModule.Add(menuOption);
                        }
                    }
                    if (!NeedRetry)
                    {
                        AMPMainStatus = ResourceUtils.Status.Ready;
                        ErrorMonitor = true;
                        RetryCount = 1;
                    }
                    if (AMPMainStatus == ResourceUtils.Status.Ready && MenuOptions.Count > 0)
                    {
                        NeedLoadModules = false;
                        //ResourceUtils.Logz(new string[2]
                        //{
                        //  "AMP",
                        //  "NOTIFY"
                        //}, new string[2]
                        //{
                        //  MenuOptions.Count.ToString() + " MODULES LOADED",
                        //  "AMP READY."
                        //});
                    }
                    else if (AMPMainStatus == ResourceUtils.Status.Error || MenuOptions.Count <= 0)
                        AMP_Commands.PrintOut("Failed to load commands");
                    //ResourceUtils.Logz(new string[2]
                    //{
                    //  "AMP",
                    //  "NOTIFY"
                    //}, new string[2]
                    //{
                    //  MenuOptions.Count.ToString() + " MODULES LOADED",
                    //  "AMP FAILED TO LOAD MODULES."
                    //}, LogType.Error);
                }
                else if (AMPMainStatus == ResourceUtils.Status.Loading && NeedRetry)
                {
                    if (RetryCount < RetryCountMax + 1)
                    {
                        int index = 0;
                        while (true)
                        {
                            int num = index;
                            int? count = RetryModule?.Count;
                            int valueOrDefault = count.GetValueOrDefault();
                            if (num < valueOrDefault & count.HasValue)
                            {
                                //ResourceUtils.Logz(new string[4]
                                //{
                                //  "AMP",
                                //  "MODULE",
                                //  "NOTIFY",
                                //  "RECHECK " + RetryCount
                                //}, new string[2]
                                //{
                                //    "NAME: " + RetryModule[index].ModuleName.ToUpper(),
                                //    "STATUS: " + RetryModule[index].ModuleStatus.ToString().ToUpper()
                                //});
                                if (RetryModule[index].ModuleStatus != ResourceUtils.Status.Ready)
                                {
                                    AMPMainStatus = ResourceUtils.Status.Loading;
                                    NeedRetry = true;
                                }
                                else if (RetryModule[index].ModuleStatus == ResourceUtils.Status.Ready)
                                {
                                    RetryModule.Remove(RetryModule[index]);
                                    if (RetryModule.Count == 0)
                                        break;
                                }
                                ++index;
                            }
                            else
                                goto label_24;
                        }
                        AMPMainStatus = ResourceUtils.Status.Ready;
                    label_24:
                        ++RetryCount;
                    }
                    if (MenuOptions.Count <= 0)
                        AMPMainStatus = ResourceUtils.Status.Error;
                    if (AMPMainStatus == ResourceUtils.Status.Ready)
                    {
                        ErrorMonitor = true;
                        RetryCount = 1;
                        //ResourceUtils.Logz(new string[2]
                        //{
                        //    "AMP",
                        //    "NOTIFY"
                        //}, new string[2]
                        //{
                        //    MenuOptions.Count.ToString() + " MODULES LOADED",
                        //    "AMP READY."
                        //});
                    }
                    else if (RetryCount >= RetryCountMax + 1)
                    {
                        //ResourceUtils.Logz(new string[2]
                        //{
                        //    "AMP",
                        //    "NOTIFY"
                        //}, new string[2]
                        //{
                        //    "MODULE NOT MOVING TO READY STATUS.",
                        //    "UNLOADING THE MODULE(S)."
                        //}, LogType.Warning);
                        foreach (AMPBaseModule ampBaseModule in RetryModule)
                        {
                            if (ampBaseModule.ModuleStatus != ResourceUtils.Status.Ready)
                            {
                                ampBaseModule.RemoveModule();
                                MenuOptions.Remove(ampBaseModule);
                            }
                        }
                        RetryModule.Clear();
                        NeedRetry = false;
                        AMPMainStatus = ResourceUtils.Status.Loading;
                    }
                }
            }
            else
                FirstLoad = false;
            if (ErrorMonitor)
            {
                int index1 = 0;
                while (true)
                {
                    int num = index1;
                    int? count = MenuOptions?.Count;
                    int valueOrDefault = count.GetValueOrDefault();
                    if (num < valueOrDefault & count.HasValue)
                    {
                        AMPBaseModule menuOption1 = MenuOptions[index1];
                        if ((menuOption1 != null ? (menuOption1.ModuleStatus == ResourceUtils.Status.Error ? 1 : 0) : 0) != 0 && !RetryModule.Contains(MenuOptions[index1]))
                        {
                            //ResourceUtils.Logz(new string[2]
                            //{
                            //    "AMP",
                            //    "NOTIFY"
                            //}, new string[2]
                            //{
                            //    "MODULE IN ERROR STATUS.",
                            //    "CHECKING MODULE: " + MenuOptions[index1].ModuleName.ToUpper()
                            //}, LogType.Warning);
                            RetryModule.Add(MenuOptions[index1]);
                        }
                        else
                        {
                            AMPBaseModule menuOption2 = this.MenuOptions[index1];
                            if ((menuOption2 != null ? (menuOption2.ModuleStatus == ResourceUtils.Status.Unload ? 1 : 0) : 0) != 0)
                            {
                                //ResourceUtils.Logz(new string[2]
                                //{
                                //  "AMP",
                                //  "NOTIFY"
                                //}, new string[1]
                                //{
                                //    "MODULE READY TO UNLOAD. UNLOADING MODULE: " + this.MenuOptions[index1].ModuleName.ToUpper()
                                //}, LogType.Warning);
                                MenuOptions[index1].RemoveModule();
                                MenuOptions.Remove(this.MenuOptions[index1]);

                            }
                        }
                        ++index1;
                    }
                    else
                        break;
                }
                List<AMPBaseModule> retryModule1 = RetryModule;
                // ISSUE: explicit non-virtual call
                if ((retryModule1 != null ? ((retryModule1.Count) > 0 ? 1 : 0) : 0) != 0 && RetryCount < RetryCountMax + 1)
                {
                    for (int index2 = 0; index2 < RetryModule.Count; ++index2)
                    {
                        if (this.RetryModule[index2].ModuleStatus == ResourceUtils.Status.Ready)
                        {
                            this.RetryModule.Remove(this.RetryModule[index2]);
                            //ResourceUtils.Logz(new string[2]
                            //{
                            //    "AMP",
                            //    "NOTIFY"
                            //}, new string[2]
                            //{
                            //    "MODULE READY.",
                            //    "MODULE: " + MenuOptions[index2].ModuleName.ToUpper()
                            //});
                            if (RetryModule.Count == 0)
                                break;
                        }
                    }
                    ++RetryCount;
                }
                else
                {
                    List<AMPBaseModule> retryModule2 = RetryModule;
                    // ISSUE: explicit non-virtual call
                    if ((retryModule2 != null ? ((retryModule2.Count) > 0 ? 1 : 0) : 0) != 0 && RetryCount >= RetryCountMax + 1)
                    {
                        foreach (AMPBaseModule ampBaseModule in RetryModule)
                        {
                            if (ampBaseModule.ModuleStatus != ResourceUtils.Status.Ready)
                            {
                                //ResourceUtils.Logz(new string[2]
                                //{
                                //  "AMP",
                                //  "NOTIFY"
                                //}, new string[2]
                                //{
                                //  "COULD NOT RESOLVE ERROR.",
                                //  "UNLOADING THE MODULE: " + ampBaseModule.ModuleName.ToUpper()
                                //}, LogType.Warning);
                                ampBaseModule.RemoveModule();
                                MenuOptions.Remove(ampBaseModule);
                            }
                        }
                        RetryModule.Clear();
                        RetryCount = 1;
                        if (MenuOptions.Count == 0)
                        {
                            this.AMPMainStatus = ResourceUtils.Status.Error;
                            //ResourceUtils.Logz(new string[2]
                            //{
                            //    "AMP",
                            //    "NOTIFY"
                            //}, new string[2]
                            //{
                            //    "NO MODULES LOADED.",
                            //    "TOOLBOX ENTERING ERROR STATE."
                            //}, LogType.Error);
                        }
                    }
                }
            }
            OnUpdate();
        }

        internal List<AMPBaseModule> GetOptions() => MenuOptions;

        public void BeginMainMenu()
        {
            moduleConsole = gameObject.AddComponent<ModConsoleOpt>();

            MenuOptions.Add(moduleConsole);
        }

        public static T ParseEnum<T>(string value) => (T)Enum.Parse(typeof(T), value, true);

        private void Init()
        {
        }

        private void OnUpdate()
        {
        }

        public void OnGUI()
        {
        }
    }
}
