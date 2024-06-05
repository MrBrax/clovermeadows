using System;
using vcrossing.Code.Npc;

namespace vcrossing.Code.Schedule.Activities;

public class BaseActivity
{

    public BaseNpc Npc { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public virtual void OnActivityStartLive()
    {

    }

    public virtual void OnActivityStartMidway()
    {

    }

    public virtual void OnWorldLoad()
    {

    }

}
