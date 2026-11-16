namespace Pos.Domain;

/// <summary>
/// Порт хранилища критериев (рубрикатор — это все критерии курса).
/// </summary>
public interface ICriterionRepository
{
    void Add(Criterion criterion);

    Criterion? GetById(string id);

    IReadOnlyList<Criterion> GetAll();
}
