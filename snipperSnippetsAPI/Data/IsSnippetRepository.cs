public interface ISnippetRepository
{
    void Save(Snippet snippet);
    Snippet GetById(int id);
}
