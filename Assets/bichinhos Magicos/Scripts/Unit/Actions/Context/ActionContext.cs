using System.Collections.Generic;

public class ActionContext
{
    public Unit source;
    public List<Unit> targets = new();

    public ActionContext(Unit source)
    {
        this.source = source;
    }
}
