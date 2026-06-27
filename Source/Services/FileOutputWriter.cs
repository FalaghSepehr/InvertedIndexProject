namespace InvertedIndexProgram;

public class FileOutputWriter : IOutputWriter
{
    private readonly string _path;

    public FileOutputWriter(string path)
    {
        _path = path;
    }
    public void WriteLine(string message)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path));
        File.AppendAllText(_path, message + Environment.NewLine);
    }
}