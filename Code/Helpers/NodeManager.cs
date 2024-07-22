using vcrossing.Code.Save;
using vcrossing.Code.Ui;

namespace vcrossing.Code.Helpers;

public partial class NodeManager : Node
{

	public static NodeManager Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public static UserInterface UserInterface => Instance.GetNode<UserInterface>( "/root/Main/UserInterface" );
	public static InventoryUi InventoryUi => Instance.GetNode<InventoryUi>( "/root/Main/UserInterface/Inventory" );
	public static WorldManager WorldManager => Instance.GetNode<WorldManager>( "/root/Main/WorldManager" );
	public static SettingsSaveData SettingsSaveData => Instance.GetNode<SettingsSaveData>( "/root/SettingsSaveData" );

}