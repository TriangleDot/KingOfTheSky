//Neither of these using statements are required for this minimal gamemode however any larger one will
using System;
using Godot;
using System.Collections.Generic;


private class CustomCommands
{
	private static KOS Self; //To access the gamemode instance
	public CustomCommands(KOS SelfArg)
	{
		Self = SelfArg;
	}


	public bool Test() //Called in the console with Gm.Test()
	{
		Console.Print("Custom commands work!");
		return true;
	}

	public bool StartGame()
	{
		
		
		Net.SteelRpc(Self, nameof(Self.StartGame)); 
		Self.StartGame();
		return true;
	}

}



public class KOS : Gamemode //Gamemode inherits Godot.Node
{
	private string[] chunks = {"{\"P\":[0,0],\"S\":[{\"T\":1,\"P\":[12,0,0]},{\"T\":1,\"P\":[-12,0,0]},{\"T\":1,\"P\":[12,0,12]},{\"T\":1,\"P\":[0,0,12]},{\"T\":1,\"P\":[-12,0,12]},{\"T\":1,\"P\":[-12,0,-12]},{\"T\":1,\"P\":[0,0,-12]},{\"T\":1,\"P\":[12,0,-12]},{\"T\":1,\"P\":[-12,0,-48]},{\"T\":1,\"P\":[-24,0,-36]},{\"T\":1,\"P\":[-36,0,-24]},{\"T\":1,\"P\":[-48,0,-12]},{\"T\":1,\"P\":[-48,0,36]},{\"T\":1,\"P\":[-36,0,36]},{\"T\":1,\"P\":[-24,0,36]},{\"T\":1,\"P\":[-12,0,36]},{\"T\":1,\"P\":[0,0,36]},{\"T\":1,\"P\":[12,0,36]},{\"T\":1,\"P\":[24,0,36]},{\"T\":1,\"P\":[36,0,36]},{\"T\":1,\"P\":[48,0,36]},{\"T\":1,\"P\":[48,0,-12]},{\"T\":1,\"P\":[36,0,-24]},{\"T\":1,\"P\":[24,0,-36]},{\"T\":1,\"P\":[12,0,-48]}]}","{\"P\":[0,-108],\"S\":[{\"T\":1,\"P\":[0,0,-60]}]}","{\"P\":[108,0],\"S\":[{\"T\":1,\"P\":[60,0,36]},{\"T\":1,\"P\":[72,0,36]},{\"T\":1,\"P\":[84,0,36]},{\"T\":1,\"P\":[96,0,36]},{\"T\":1,\"P\":[84,0,24]},{\"T\":1,\"P\":[72,0,12]},{\"T\":1,\"P\":[60,0,0]}]}","{\"P\":[-108,0],\"S\":[{\"T\":1,\"P\":[-60,0,0]},{\"T\":1,\"P\":[-72,0,12]},{\"T\":1,\"P\":[-84,0,24]},{\"T\":1,\"P\":[-96,0,36]},{\"T\":1,\"P\":[-84,0,36]},{\"T\":1,\"P\":[-72,0,36]},{\"T\":1,\"P\":[-60,0,36]}]}","{\"P\":[-432,0],\"S\":[{\"T\":3,\"P\":[-480,138,0],\"R\":[0,90,0]},{\"T\":2,\"P\":[-486,126,0],\"R\":[0,-90,0]}]}", "{\"P\":[-540,0],\"S\":[{\"T\":1,\"P\":[-492,132,0]},{\"T\":3,\"P\":[-492,138,12]},{\"T\":3,\"P\":[-504,138,0],\"R\":[0,-90,0]},{\"T\":3,\"P\":[-492,138,-12],\"R\":[0,-180,0]},{\"T\":1,\"P\":[-492,120,0]},{\"T\":2,\"P\":[-498,126,0],\"R\":[0,-90,0]},{\"T\":2,\"P\":[-492,126,6]},{\"T\":2,\"P\":[-492,126,-6],\"R\":[0,-180,0]}]}"};
	private bool team = false;
	private int readied = 0;
	private int start_time;
	private Label start_label = null;
	private bool start_running;
	private Label l;
	private bool running;

	
	public override void _Ready() //Provided by Godot.Node
	{
		if(Net.Work.IsNetworkServer())
			Net.SteelRpc(Scripting.Self, nameof(Scripting.RequestGmLoad), OwnName); //Load same gamemode on all connected clients

		API.Gm = new CustomCommands(this);
		loadWorld(chunks);
	}

	public void loadWorld(string[] chunkz) {
		World.Clear();
		World.DefaultPlatforms();
		foreach (string chunk in chunkz) {
			World.LoadChunk(chunk);
		}
	}


	public override void OnPlayerConnect(int Id)
	{
		if(Net.Work.IsNetworkServer())
			Scripting.Self.RpcId(Id, nameof(Scripting.RequestGmLoad), OwnName); //Load same gamemode on newly connected client
	}


	public override void OnUnload()
	{
		if(Net.Work.IsNetworkServer())
			Net.SteelRpc(Scripting.Self, nameof(Scripting.RequestGmUnload)); //Unload gamemode on all clients

		API.Gm = new API.EmptyCustomCommands(); //Could also set to null but this gives better error message when empty
		l.QueueFree();

	}

	[Remote]
	public void OnReady(bool am)
	{
		readied ++;
		Console.Print($"{readied} out of {Net.Players.Count} ready!");
		if (readied == Net.Players.Count) {
			int startTime = OS.GetTicksMsec()+500;
			Net.SteelRpc(this, nameof(this.SetStartTime), startTime);
			SetStartTime(startTime);

		}
	}

	[Remote]
	public void SetStartTime(int time) {
		Console.Print($"Starting at time {time}");
		/*start_time = time;
		start_label = new Label();
		start_label.RectPosition = new Vector2(500,0);
		start_label.Text = ((start_time-OS.GetTicksMsec())/1000).ToString();
		GetNode("/root").AddChild(start_label);*/
		start_running = true;
		
		
	}

	public override void _Process(float delta) {
		if (start_running) {
			/*start_label.QueueFree();
			start_label = new Label();
			start_label.RectPosition = new Vector2(500,0);
		
			
			start_label.Text = ((start_time-OS.GetTicksMsec())/1000).ToString();
			GeNode("/root").AddChild(start_label);*/
			if (OS.GetTicksMsec() > start_time) {
				
				start_running = false;
				//start_label.QueueFree();
				running = true;
				Console.Print("Starting Game!");
				start_time = OS.GetTicksMsec();
				if (team == false) {
					Net.Players[Net.Work.GetNetworkUniqueId()].Translation = new Vector3(-489,132,1);
				}
			}
		}
		if (running) {
			if (team == true) {
				var x = Net.Players[Net.Work.GetNetworkUniqueId()];
				if (World.GetChunkPos(x.Translation) == new Vector3(-540,0,0) && x.Translation.y > 125 && x.Translation.y < 140) {
					int end_time = OS.GetTicksMsec();
					Net.SteelRpc(this, nameof(this.BlueWon), end_time);
					BlueWon(end_time);
				}
			}
		}
	}
	// Player spawn vector 



	[Remote]
	public void BlueWon(int end_time) {
		running = false;
		l.Text = $"Blue won in {(end_time-start_time)/1000} seconds";

	}

	[Remote]
	public void StartGame()
	{
		Console.Print("Starting Game!");
		if (Net.Work.IsNetworkServer())
		{
			List<int> blueteam = new List<int>();
			List<int> redteam  = new List<int>();
			bool team = true;
			foreach (int id in Net.PeerList) {
				if (team) {
					blueteam.Add(id);
					team = false;

				}
				else {
					redteam.Add(id);
					team = true;
				}

			}
			Net.SteelRpc(this, nameof(this.AssignTeam),blueteam.ToArray(),redteam.ToArray());
			//API.Gm.CallRpc(nameof(this.AssignTeam),blueteam,redteam);
			AssignTeam(blueteam.ToArray(),redteam.ToArray());
		}
	}

	[Remote]
	public void AssignTeam(int[] blueteam, int[] redteam) {
		foreach (int id in redteam) {
			if (id == GetTree().GetNetworkUniqueId()) {
				team = false;
				l = new Label();
				l.Text = "You are on the RED team! Defend the hill!";
				GetNode("/root").AddChild(l);
				Net.Players[Net.Work.GetNetworkUniqueId()].SetFly(false);
			}
		}
		foreach (int id in blueteam) {
			if (id == GetTree().GetNetworkUniqueId()) {
				team = true;
				l = new Label();
				l.Text = "You are on the BLUE team! Take the hill!";
				GetNode("/root").AddChild(l);
				Net.Players[Net.Work.GetNetworkUniqueId()].SetFly(false);
				
			}
		}
		Net.SteelRpc(this, nameof(this.OnReady), true);
		OnReady(true);
	}
}


return new KOS();