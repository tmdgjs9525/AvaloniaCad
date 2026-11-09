namespace KoDrawing.Core.Entities;

public abstract class Entity
{
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>소속 레이어. Guid.Empty면 문서에 추가될 때 현재 레이어로 지정된다.</summary>
    public Guid LayerId { get; set; }
}