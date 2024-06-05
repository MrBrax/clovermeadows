using System;

namespace vcrossing.Code.Schedule.Activities;

public class SleepActivity : BaseActivity
{
    public override void OnActivityStartLive()
    {

        // walk to house if not already there
        // walk to bed
        // enter bed
        // sleep

    }

    public override void OnActivityStartMidway()
    {

    }

    public override void OnWorldLoad()
    {
        // teleport to bed
        // sleep
    }

}
