using System.Text.Json;
using Libraries.Models;

namespace Libraries;


/// <summary>
/// This class provides various functions for handling files
/// </summary>
public static class FileHandler
{
    public static string Bin_Directory;

    public static void Save_Object<TSavable>(TSavable savable, string? file_path = null) where TSavable : SavableObject
    {
        file_path ??= Path.Join(Bin_Directory, savable.GetType().Name + ".json");

        // Convert object to json
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        string settings_json = JsonSerializer.Serialize(savable, options);

        // Write to file
        File.WriteAllText(file_path, settings_json);
    }

    /// <summary>
    /// Read all contents of a file, raise an exception if the file doesn't exist
    /// </summary>
    /// <param name="file_path">The path to the file</param>
    /// <returns>Returns the file contents</returns>
    public static string Read_File_Contents(string file_path)
    {
        // Check if settings exsist, else run setup
        if (!File.Exists(file_path))
        {
            throw new FileNotFoundException("File missing!");
        }

        // Load string from file and convert it to object
        // Update global settings object
        string file_contents = File.ReadAllText(file_path);

        return file_contents;
    }

    public static TSavable Deserialize_To_Object<TSavable>(string json_string) where TSavable : SavableObject
    {
        try
        {
            TSavable savable = JsonSerializer.Deserialize<TSavable>(json_string);
            return savable;
        }
        catch
        {
            throw new FileLoadException("JSON file may be corrupt!");
        }
    }

    public static TSavable Load_To_Object<TSavable>(string file_path) where TSavable : SavableObject
    {
        string json_string;

        try
        {
            json_string = Read_File_Contents(file_path);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"File: {file_path} missing!");
        }

        try
        {
            return Deserialize_To_Object<TSavable>(json_string);
        }
        catch (FileLoadException flex)
        {
            throw new FileNotFoundException(flex.Message);
        }
    }
}

/// <summary>
/// This class provides functions to collect debug data about the system
/// </summary>
public class DataCollector
{
    /// <summary>
    /// Get the name of the OS
    /// </summary>
    /// <returns>Returns the name of the OS as a string</returns>
    public static string Get_OS_Description()
    {
        return System.Runtime.InteropServices.RuntimeInformation.OSDescription;
    }

    /// <summary>
    /// Get the version of the OS
    /// </summary>
    /// <returns>Returns the version of the OS as a string</returns>
    public static string Get_OS_Version()
    {
        return Environment.OSVersion.Version.ToString();
    }

    /// <summary>
    /// Get the architecture of the system
    /// </summary>
    /// <returns>Returns the architecture of the system as a string</returns>
    public static string Get_OS_Architecture()
    {
        return System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString();
    }

    /// <summary>
    /// Get the count of CPU cores
    /// </summary>
    /// <returns>Returns the count of CPU cores as integer</returns>
    public static int Get_CPU_Count()
    {
        return Environment.ProcessorCount;
    }

    /// <summary>
    /// Get the count of GPUs
    /// </summary>
    /// <returns>Returns the count of GPUs as integer</returns>
    public static int Get_GPU_Count()
    {
        return -1;
    }

    /// <summary>
    /// Get the amount of System RAM
    /// </summary>
    /// <returns>Returns the amount of RAM as integer</returns>
    public static int Get_RAM()
    {
        return -1;
    }
}