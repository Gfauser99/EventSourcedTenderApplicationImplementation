namespace Core.Infrastructure;

public interface IFileSystem
{
    void CreateFile(string path, string contents);
    string ReadFile(string path);
    void DeleteFile(string path);
    bool FileExists(string path);
}

public class FileSystem : IFileSystem
{
    public void CreateFile(string path, string contents)
    {
        //Should create a file in folder from path with a filename
        File.WriteAllText(path, contents);
    }
    //Could be used for further functionality but no usage currently
    public string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }
}

