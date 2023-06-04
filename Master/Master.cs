﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

class Master
{
    #region Global Variables
    // Initialize global variables
    // Initialize global File names, directories
    public static string SCRIPT_DIRECTORY = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    public static string LOGS_DIRECTORY = "";
    public static string LOGS_FILE = "";
    public static string PROJECT_DIRECTORY = "";
    public static string PROJECT_EXTENSION = "prfp";
    public static string SETTINGS_FILE = "";
    public static string DATA_FILE = "";

    // Create global objects and variables
    public static float VERSION = 1.0f;
    public static Settings SETTINGS;
    public static PRF_Data PRF_DATA;
    public static Project PROJECT;
    public static List<int> frames_left = new List<int>();
    #endregion

    // Runs at start
    static void Main(string[] args)
    {
        // log the start time
        DateTime start_time = DateTime.Now;

        // Get log directory name and create it
        LOGS_DIRECTORY = Path.Join(SCRIPT_DIRECTORY, "logs");
        if (!Directory.Exists(LOGS_DIRECTORY))
        {
            Directory.CreateDirectory(LOGS_DIRECTORY);
        }

        // Get the name for the current log, settings and data
        LOGS_FILE = Path.Join(LOGS_DIRECTORY, ("master_" + start_time.ToString("HHmmss") + ".txt"));
        SETTINGS_FILE = Path.Join(SCRIPT_DIRECTORY, "master_settings.json");
        DATA_FILE = Path.Join(SCRIPT_DIRECTORY, "master_data.json");

        // Main loop start
        Load_Settings();
        Collect_Data();
        Main_Menu();
    }

    public static void Main_Menu()
    {
        // Create list with all options and hand it to Menu()
        List<string> items = new List<string>
        {
            "New project",
            "Load project from disk",
            "Re-run setup",
            "Visit documentation",
            "Get help on Discord",
            "Donate - Out of order",
            "Exit"
        };

        while (true)
        {
            // Show the Menu and grab the selection
            string selection = Menu(items, new List<string> { });

            // Compare selection with options and execute function
            if (selection == items[0])
            {
                Project_Setup();
            }

            else if (selection == items[1])
            {

            }

            else if (selection == items[2])
            {
                // Run the first time setup and come back here
                First_Time_Setup();
            }

            else if (selection == items[3])
            {
                // Open documentation on Github.com
                Process.Start(new ProcessStartInfo("https://github.com/PidgeonTools/PidgeonRenderFarm") { UseShellExecute = true });
                Main_Menu();
            }

            else if (selection == items[4])
            {
                // Open Discord invite in browser
                Process.Start(new ProcessStartInfo("https://discord.gg/cnFdGQP") { UseShellExecute = true });
                Main_Menu();
            }

            else if (selection == items[5])
            {

            }

            else
            {
                // Exit PRF
                Environment.Exit(1);
            }
        }
    }

    public static void Render_Project()
    {
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        try
        {

            // Create a Socket that will use Tcp protocol
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method
            listener.Bind(localEndPoint);
            // Specify how many requests a Socket can listen before it gives Server busy response.
            // We will listen 10 requests at a time
            listener.Listen(10);

            Console.WriteLine("Waiting for a connection...");
            Socket handler = listener.Accept();

            // Incoming data from the client.
            string data = "";
            byte[] bytes;

            while (true)
            {
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }

            Console.WriteLine("Text received : {0}", data);

            byte[] msg = Encoding.ASCII.GetBytes(data);
            handler.Send(msg);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\n Press any key to continue...");
        Console.ReadKey();
    }

    #region Menu_Display
    // Open a semi-graphical menu allowing easy user input
    public static string Menu(List<string> options, List<string> headlines)
    {
        // Hide cursor
        int selected = 0;
        Console.CursorVisible = false;

        // Create and fill list with given options
        List<Menu_Item> items = new List<Menu_Item>();
        foreach (string option in options)
        {
            items.Add(new Menu_Item(option));
        }// Create empty settings object
        Settings new_settings = new Settings();
        // Make the 1st option the default
        items[0].selected = true;

        // Infinite loop
        while (true)
        {
            // Clear everything and show the top bar
            Console.Clear();
            Show_Top_Bar();

            // Show an additional promt if given
            if (headlines.Count != 0)
            {
                // Print every line
                foreach (string headline in headlines)
                {
                    Console.WriteLine(headline);
                }
                //Console.WriteLine("#--------------------------------------------------------------#");
                //Console.WriteLine("");
            }

            // GO through all items and color them depending on the selection
            foreach (Menu_Item item in items)
            {
                if (item.selected)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                }

                Console.WriteLine(item.text);

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }

            // Wait for user input
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            ConsoleKey key = keyInfo.Key;

            // If possible go up in list
            if (key == ConsoleKey.UpArrow && selected != 0)
            {
                items[selected].selected = false;

                selected--;

                items[selected].selected = true;
            }

            // If possible go down in list
            else if (key == ConsoleKey.DownArrow && selected != (items.Count() - 1))
            {
                items[selected].selected = false;

                selected++;

                items[selected].selected = true;
            }

            // Return the current selection and break out of the loop
            // Show the cursor
            else if (key == ConsoleKey.Enter)
            {
                Console.Clear();
                Console.CursorVisible = true;
                return items[selected].text;
            }
        }
    }

    // Print the top bar
    public static void Show_Top_Bar()
    {
        Console.WriteLine("Pidgeon Render Farm");
        Console.WriteLine("Join the Discord server for support - https://discord.gg/cnFdGQP");
        Console.WriteLine("");
        Console.WriteLine("#--------------------------------------------------------------#");
        Console.WriteLine("");
    }
    #endregion

    #region Setup
    public static void First_Time_Setup()
    {
        // Create empty settings object
        Settings new_settings = new Settings();
        // Removes the need to type "new List<string> { "Yes", "No" }" every time
        List<string> basic_bool = new List<string> { "Yes", "No" };

        // Write version - for updating in the future
        new_settings.version = VERSION;

        // Enable logging
        // Use Menu() to grab user input
        new_settings.enable_logging = Parse_Bool(Menu(basic_bool, new List<string> { "Enable logging? (It is recommended to turn this on. To see whats included please refer to the documentation!)" }));

        // Port
        // Let the user input a valid port
        // If emtpy, then use default port
        Show_Top_Bar();
        Console.WriteLine("Which Port to use? (Default: 8080):");
        string user_input = Console.ReadLine();
        while (!Is_Port(user_input))
        {
            if (user_input == "")
            {
                user_input = "8080";
                break;
            }

            Console.WriteLine("Please input a whole number between 1 and 65536");
            user_input = Console.ReadLine();
        }
        new_settings.port = Math.Abs(int.Parse(user_input));
        Console.Clear();

        // Blender Executable
        // Check if the file exsists
        Show_Top_Bar();
        Console.WriteLine("Where is you blender.exe stored? (It's recommended not to use blender-launcher.exe)");
        user_input = Console.ReadLine();
        while (!File.Exists(user_input))
        {
            Console.WriteLine("Please input the path to blender.exe (Without '')");
            user_input = Console.ReadLine();
        }
        new_settings.blender_executable = user_input;

        // Keep output
        // Use Menu() to grab user input
        new_settings.keep_output = Parse_Bool(Menu(basic_bool, new List<string> { "Keep the files received from the clients?" }));

        // Use FTP
        // Use Menu() to grab user input
        new_settings.use_ftp = Parse_Bool(Menu(basic_bool, new List<string> { "Use 'File Transfer Protocol' instead of sockets for file distribution?" }));

        // Use ZIP
        // Use Menu() to grab user input
        new_settings.use_zip = Parse_Bool(Menu(basic_bool, new List<string> { "Zip/compress files before distributing? (might save some bandwitdh at the cost of image quality)" }));

        // Data collection
        // Use Menu() to grab user input
        new_settings.collect_data = Parse_Bool(Menu(basic_bool, new List<string> { "Allow us to collect data? (We have no acess to it, even if you enter yes!) " }));

        // Save the settings
        Save_Settings(new_settings);
    }

    // Save settings based on a new set
    public static void Save_Settings(Settings new_settings)
    {
        // Update global settings object
        SETTINGS = new_settings;

        // Convert object to json
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(SETTINGS, options);

        // Write to file
        File.WriteAllText(SETTINGS_FILE, jsonString);
    }

    // Load the settings
    public static void Load_Settings(string custom_file = "")
    {
        if (custom_file == "")
        {
            custom_file = SETTINGS_FILE;
        }

        // Check if settings exsist, else run setup
        if (!File.Exists(custom_file))
        {
            First_Time_Setup();
            return;
        }

        // Load string from file and convert it to object
        // Update global settings object
        string json_string = File.ReadAllText(custom_file);
        SETTINGS = JsonSerializer.Deserialize<Settings>(json_string);
    }
    #endregion

    #region Project_Setup
    // Run the setup for the project
    public static void Project_Setup()
    {
        // Create empty project
        Project new_project = new Project();

        // +1 on data file
        Save_Data();
        // Generate ID based on project number
        new_project.id = PRF_DATA.projects.ToString();

        // Blend file
        // Let user input a valid Blender project file
        Show_Top_Bar();
        Console.WriteLine("Where .blend stored? (Be sure to actually use a .blend)");
        string user_input = Console.ReadLine();
        while (!File.Exists(user_input) && !user_input.EndsWith(".blend"))
        {
            Console.WriteLine("Please input the path to your .blend (Without '')");
            user_input = Console.ReadLine();
        }
        new_project.full_path_blend = user_input;

        // Render test frame (for time)
        // Use Menu() to grab user input
        bool test_render = Parse_Bool(Menu(new List<string> { "Yes", "No" },
                                           new List<string> { "Render a test frame? (Will take some time, for client option 'Maximum time per frame')" }));

        // Generate a video file
        // Use Menu() to grab user input
        new_project.video_generate = Parse_Bool(Menu(new List<string> { "Yes", "No" },
                                                     new List<string> { "Generate a video file? (MP4-Format, FFMPEG has to be installed!)" }));

        // Only required if we generate a video
        if (new_project.video_generate)
        {
            // Video rate control type
            // Use Menu() to grab user input
            new_project.video_rate_control = Menu(new List<string> { "CRF", "CBR" },
                                                  new List<string> { "What Video Rate Control to use?" });

            // Video rate control value (e.g. bitrate)
            // Let user input a valid value
            Show_Top_Bar();
            Console.WriteLine("Video Rate Control Value: (CRF - lower is better; CBR - higher is better)");
            user_input = Console.ReadLine();
            while (!int.TryParse(user_input, out _))
            {
                Console.WriteLine("Please input a whole number");
                user_input = Console.ReadLine();
            }
            new_project.video_rate_control_value = Math.Abs(int.Parse(user_input));
            Console.Clear();

            // Resize the video
            // Use Menu() to grab user input
            new_project.video_resize = Parse_Bool(Menu(new List<string> { "Yes", "No" },
                                                       new List<string> { "Rescale the video?" }));

            // New video witdh/x
            // Let user input a valid resolution
            Show_Top_Bar();
            Console.WriteLine("New video width:");
            user_input = Console.ReadLine();
            while (!int.TryParse(user_input, out _))
            {
                Console.WriteLine("Please input a whole number");
                user_input = Console.ReadLine();
            }
            new_project.video_x = Math.Abs(int.Parse(user_input));
            Console.Clear();

            // New video height/y
            // Let user input a valid resolution
            Show_Top_Bar();
            Console.WriteLine("New video height:");
            user_input = Console.ReadLine();
            while (!int.TryParse(user_input, out _))
            {
                Console.WriteLine("Please input a whole number");
                user_input = Console.ReadLine();
            }
            new_project.video_y = Math.Abs(int.Parse(user_input));
            Console.Clear();

            // Chunks
            // Let user input a valid size
            Show_Top_Bar();
            Console.WriteLine("Chunk size:");
            user_input = Console.ReadLine();
            while (!int.TryParse(user_input, out _))
            {
                Console.WriteLine("Please input a whole number");
                user_input = Console.ReadLine();
            }
            new_project.video_y = Math.Abs(int.Parse(user_input));
            Console.Clear();
        }

        // Get project directory name and create it
        PROJECT_DIRECTORY = Path.Join(SCRIPT_DIRECTORY, new_project.id);
        Directory.CreateDirectory(PROJECT_DIRECTORY);

        // Create a command for blender to optain some variables
        //string command = SETTINGS.blender_executable;
        string args = "-b ";
        args += new_project.full_path_blend;
        args += " -P ";
        args += "BPY.py";
        args += " -- ";
        args += PROJECT_DIRECTORY;
        if (test_render)
        {
            args += " 1";
        }
        else
        {
            args += " 0";
        }

        // Use Blender to obtain informations about the project
        Process process = new Process();
        // Set Blender as executable
        process.StartInfo.FileName = SETTINGS.blender_executable;
        // Use the command string as args
        process.StartInfo.Arguments = args;
        process.StartInfo.CreateNoWindow = true;
        // Redirect output to log Blenders output
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        // Print and log the output
        string cmd_output = "";
        while (!process.HasExited)
        {
            cmd_output += process.StandardOutput.ReadToEnd();
            Console.WriteLine(cmd_output);
        }

        // Read the output
        string json_string = File.ReadAllText(Path.Join(PROJECT_DIRECTORY, "vars.json"));
        Project_Data project_data = JsonSerializer.Deserialize<Project_Data>(json_string);

        // Apply the values to project object
        new_project.blender_version = project_data.blender_version;
        new_project.render_engine = project_data.render_engine;
        new_project.time_per_frame = project_data.render_time;
        new_project.output_file_format = project_data.file_format;
        new_project.first_frame = project_data.first_frame;
        new_project.last_frame = project_data.last_frame;
        new_project.frames_total = project_data.last_frame - (project_data.first_frame - 1);

        // Append every frame to frames_left
        for (int frame = new_project.first_frame; frame <= new_project.last_frame; frame++)
        {
            frames_left.Add(frame);
        }

        // Save the project
        Save_Project(new_project);

        // Start rendering
    }

    // Save the project
    public static void Save_Project(Project new_project)
    {
        // Update the global object
        PROJECT = new_project;

        // Convert object to json
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(PROJECT, options);

        // Write to file
        File.WriteAllText(Path.Combine(PROJECT_DIRECTORY, PROJECT.id), jsonString);
    }

    // Load a project
    public static void Load_Project(string project_file)
    {
        // Prevent file read errors
        if (!File.Exists(PROJECT.full_path_blend))
        {
            Project_Setup();
        }

        // Read json from file
        // Convert json to object
        // Update global project object
        // Update global project directory
        string json_string = File.ReadAllText(project_file);
        PROJECT = JsonSerializer.Deserialize<Project>(json_string);
        PROJECT_DIRECTORY = Path.Join(SCRIPT_DIRECTORY, PROJECT.id);

        frames_left = new List<int>();

        for (int frame = PROJECT.first_frame; frame < PROJECT.last_frame; frame++)
        {
            if (!PROJECT.frames_complete.Contains(frame))
            {
                frames_left.Add(frame);
            }
        }
    }
    #endregion

    #region Data Handeling
    // Write text to log file
    public static void Write_Log(string content)
    {
        // Make sure file exsists
        if (!File.Exists(LOGS_FILE))
        {
            File.Create(LOGS_FILE).Close();
        }

        // Append line break
        content += Environment.NewLine;

        // Append new line to file
        File.AppendAllText(LOGS_FILE, content);
    }

    // Collect system data
    public static void Collect_Data()
    {
        // Create empty PRF_Data object
        PRF_Data new_data = new PRF_Data();

        // ONLY proceed if the user agrees
        if (SETTINGS.collect_data)
        {
            // Gather informations and add to data object
            new_data.version = VERSION;
            new_data.os = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        }

        // Save the obtained data
        Save_Data(new_data);
    }

    // Save data object
    public static void Save_Data(PRF_Data new_data)
    {
        // Update global PRF_DATA object
        PRF_DATA = new_data;

        // Convert object to json
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json_string = JsonSerializer.Serialize(PRF_DATA, options);

        // Write json to file
        File.WriteAllText(DATA_FILE, json_string);
    }

    // Add 1 to projects variable
    public static void Save_Data()
    {
        // +1 here
        PRF_DATA.projects++;

        // Convert object to json
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json_string = JsonSerializer.Serialize(PRF_DATA, options);

        // Write json to file
        File.WriteAllText(DATA_FILE, json_string);
    }

    // Load PRF_DATA
    public static void Load_Data(PRF_Data new_data)
    {
        // Prevent read errors
        if (!File.Exists(DATA_FILE))
        {
            Collect_Data();
            return;
        }

        // Read json from file
        // Convert json to object
        // Update global PRF_DATA object
        string json_string = File.ReadAllText(DATA_FILE);
        PRF_DATA = JsonSerializer.Deserialize<PRF_Data>(json_string);
    }
    #endregion

    #region Helpers
    // Check if string is a valid port
    public static bool Is_Port(string port)
    {
        // Check if string is number
        if (!int.TryParse(port, out _))
        {
            return false;
        }

        // Check if number is between 1 and 65535
        return (Math.Abs(int.Parse(port)) >= 1 && Math.Abs(int.Parse(port)) <= 65535);
    }

    // Convert string to bool, accepts default
    public static dynamic Parse_Bool(string value, bool def = true)
    {
        // If it is a human yes, return computer true
        if (new List<string>() { "true", "yes", "y", "1" }.Any(s => s.Contains(value.ToLower())))
        {
            return true;
        }

        // If it is a human no, return computer false
        else if (new List<string>() { "false", "no", "n", "0" }.Any(s => s.Contains(value.ToLower())))
        {
            return false;
        }

        // If human is not sure, return default value
        else if (new List<string>() { "", null }.Any(s => s.Contains(value.ToLower())))
        {
            return def;
        }

        // if none applies, return error
        return null;
    }
    #endregion
}

public class Server
{
    public Server()
    {

    }

    public void Client_Handler()
    {

    }
}

#region Objects
// Settings object class
public class Settings
{
    public float version { get; set; } = 0.0f;
    public int port { get; set; } = 8080;
    public string blender_executable { get; set; }
    public bool keep_output { get; set; }
    public bool use_ftp { get; set; }
    public bool use_zip { get; set; }
    public bool collect_data { get; set; }
    public bool enable_logging { get; set; }
}

// Project object class
public class Project
{
    public string id { get; set; }
    public float blender_version { get; set; }
    public string full_path_blend { get; set; }
    public string render_engine { get; set; }
    public string output_file_format { get; set; }
    public bool video_generate { get; set; }
    public int video_fps { get; set; }
    public string video_rate_control { get; set; } // CBR, CRF
    public int video_rate_control_value { get; set; }
    public bool video_resize { get; set; }
    public int video_x { get; set; }
    public int video_y { get; set; }
    public int Chunks { get; set; } = 0;
    public float time_per_frame { get; set; }
    public int ram_use { get; set; } = 0;
    public int first_frame { get; set; }
    public int last_frame { get; set; }
    public int frames_total { get; set; }
    public List<int> frames_complete { get; set; } = new List<int>();
}

// Project_Data object class
public class Project_Data
{
    public float blender_version { get; set; }
    public string render_engine { get; set; }
    public float render_time { get; set; } = 0.0f;
    public int ram_use { get; set; } = 0;
    public string file_format { get; set; }
    public int first_frame { get; set; }
    public int last_frame { get; set; }
}

// PRF_Data object class
public class PRF_Data
{
    public float version { get; set; } = 0.0f;
    public string os { get; set; } = "No data";
    public List<string> cpus { get; set; } = new List<string> { "No data" };
    public List<string> gpus { get; set; } = new List<string> { "No data" };
    public int ram { get; set; } = 0;
    public int projects { get; set; } = 0;
}

// Menu_Item object class
public class Menu_Item
{
    public string text { get; set; }
    public bool selected { get; set; } = false;

    public Menu_Item(string item_text = "")
    {
        text = item_text;
    }
}
#endregion
