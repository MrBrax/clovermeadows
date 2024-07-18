using System;

namespace vcrossing.Code.Items;

public interface IDiggable
{

    public bool CanDig();

    public bool GiveItemWhenDug();

}
