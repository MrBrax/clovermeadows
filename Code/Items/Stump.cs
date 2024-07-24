using System;
using vcrossing.Code.Interfaces;

namespace vcrossing.Code.Items;

public partial class Stump : WorldItem, IDiggable
{

    public bool CanDig()
    {
        return true;
    }

    public bool GiveItemWhenDug()
    {
        return false;
    }

}
