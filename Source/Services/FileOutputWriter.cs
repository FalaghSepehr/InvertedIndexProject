namespace InvertedIndexProgram;

public class FileOutputWriter : IOutputWriter
{
    private readonly string _path;
    private bool _initialized;

    public FileOutputWriter(string path)
    {
        _path = path;
    }
    public void WriteLine(string message)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path));
        if (!_initialized)
        {
            File.WriteAllText(_path, string.Empty);
        }
        File.AppendAllText(_path, message + Environment.NewLine);
    }
}