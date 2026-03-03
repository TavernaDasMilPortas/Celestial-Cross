public class AttackContext : ActionContext
{
    public int damage;

    public AttackContext(Unit source, int damage)
        : base(source)
    {
        this.damage = damage;
    }
}