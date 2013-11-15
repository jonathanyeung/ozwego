﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WorkerRole.Datacore
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="ozwego-db")]
	public partial class OzwegoDataClassesDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertfriendRequest(friendRequest instance);
    partial void UpdatefriendRequest(friendRequest instance);
    partial void DeletefriendRequest(friendRequest instance);
    partial void Insertfriendship(friendship instance);
    partial void Updatefriendship(friendship instance);
    partial void Deletefriendship(friendship instance);
    partial void Insertuser_game(user_game instance);
    partial void Updateuser_game(user_game instance);
    partial void Deleteuser_game(user_game instance);
    partial void Insertgame(game instance);
    partial void Updategame(game instance);
    partial void Deletegame(game instance);
    partial void Insertuser(user instance);
    partial void Updateuser(user instance);
    partial void Deleteuser(user instance);
    #endregion
		
		public OzwegoDataClassesDataContext() : 
				base(global::WorkerRole.Properties.Settings.Default.ozwego_dbConnectionString1, mappingSource)
		{
			OnCreated();
		}
		
		public OzwegoDataClassesDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public OzwegoDataClassesDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public OzwegoDataClassesDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public OzwegoDataClassesDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<friendRequest> friendRequests
		{
			get
			{
				return this.GetTable<friendRequest>();
			}
		}
		
		public System.Data.Linq.Table<friendship> friendships
		{
			get
			{
				return this.GetTable<friendship>();
			}
		}
		
		public System.Data.Linq.Table<user_game> user_games
		{
			get
			{
				return this.GetTable<user_game>();
			}
		}
		
		public System.Data.Linq.Table<game> games
		{
			get
			{
				return this.GetTable<game>();
			}
		}
		
		public System.Data.Linq.Table<user> users
		{
			get
			{
				return this.GetTable<user>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.friendRequest")]
	public partial class friendRequest : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _from_user;
		
		private int _to_user;
		
		private System.Nullable<System.DateTime> _creation_time;
		
		private EntityRef<user> _user;
		
		private EntityRef<user> _user1;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void Onfrom_userChanging(int value);
    partial void Onfrom_userChanged();
    partial void Onto_userChanging(int value);
    partial void Onto_userChanged();
    partial void Oncreation_timeChanging(System.Nullable<System.DateTime> value);
    partial void Oncreation_timeChanged();
    #endregion
		
		public friendRequest()
		{
			this._user = default(EntityRef<user>);
			this._user1 = default(EntityRef<user>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_from_user", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int from_user
		{
			get
			{
				return this._from_user;
			}
			set
			{
				if ((this._from_user != value))
				{
					if (this._user.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.Onfrom_userChanging(value);
					this.SendPropertyChanging();
					this._from_user = value;
					this.SendPropertyChanged("from_user");
					this.Onfrom_userChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_to_user", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int to_user
		{
			get
			{
				return this._to_user;
			}
			set
			{
				if ((this._to_user != value))
				{
					if (this._user1.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.Onto_userChanging(value);
					this.SendPropertyChanging();
					this._to_user = value;
					this.SendPropertyChanged("to_user");
					this.Onto_userChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_creation_time", DbType="DateTime")]
		public System.Nullable<System.DateTime> creation_time
		{
			get
			{
				return this._creation_time;
			}
			set
			{
				if ((this._creation_time != value))
				{
					this.Oncreation_timeChanging(value);
					this.SendPropertyChanging();
					this._creation_time = value;
					this.SendPropertyChanged("creation_time");
					this.Oncreation_timeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_friendRequest", Storage="_user", ThisKey="from_user", OtherKey="ID", IsForeignKey=true)]
		public user user
		{
			get
			{
				return this._user.Entity;
			}
			set
			{
				user previousValue = this._user.Entity;
				if (((previousValue != value) 
							|| (this._user.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._user.Entity = null;
						previousValue.friendRequests.Remove(this);
					}
					this._user.Entity = value;
					if ((value != null))
					{
						value.friendRequests.Add(this);
						this._from_user = value.ID;
					}
					else
					{
						this._from_user = default(int);
					}
					this.SendPropertyChanged("user");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_friendRequest1", Storage="_user1", ThisKey="to_user", OtherKey="ID", IsForeignKey=true)]
		public user user1
		{
			get
			{
				return this._user1.Entity;
			}
			set
			{
				user previousValue = this._user1.Entity;
				if (((previousValue != value) 
							|| (this._user1.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._user1.Entity = null;
						previousValue.friendRequests1.Remove(this);
					}
					this._user1.Entity = value;
					if ((value != null))
					{
						value.friendRequests1.Add(this);
						this._to_user = value.ID;
					}
					else
					{
						this._to_user = default(int);
					}
					this.SendPropertyChanged("user1");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.friendship")]
	public partial class friendship : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _user1;
		
		private int _user2;
		
		private System.Nullable<System.DateTime> _creation_time;
		
		private EntityRef<user> _user;
		
		private EntityRef<user> _user3;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void Onuser1Changing(int value);
    partial void Onuser1Changed();
    partial void Onuser2Changing(int value);
    partial void Onuser2Changed();
    partial void Oncreation_timeChanging(System.Nullable<System.DateTime> value);
    partial void Oncreation_timeChanged();
    #endregion
		
		public friendship()
		{
			this._user = default(EntityRef<user>);
			this._user3 = default(EntityRef<user>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_user1", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int user1
		{
			get
			{
				return this._user1;
			}
			set
			{
				if ((this._user1 != value))
				{
					if (this._user.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.Onuser1Changing(value);
					this.SendPropertyChanging();
					this._user1 = value;
					this.SendPropertyChanged("user1");
					this.Onuser1Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_user2", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int user2
		{
			get
			{
				return this._user2;
			}
			set
			{
				if ((this._user2 != value))
				{
					if (this._user3.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.Onuser2Changing(value);
					this.SendPropertyChanging();
					this._user2 = value;
					this.SendPropertyChanged("user2");
					this.Onuser2Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_creation_time", DbType="DateTime")]
		public System.Nullable<System.DateTime> creation_time
		{
			get
			{
				return this._creation_time;
			}
			set
			{
				if ((this._creation_time != value))
				{
					this.Oncreation_timeChanging(value);
					this.SendPropertyChanging();
					this._creation_time = value;
					this.SendPropertyChanged("creation_time");
					this.Oncreation_timeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_friendship", Storage="_user", ThisKey="user1", OtherKey="ID", IsForeignKey=true)]
		public user user
		{
			get
			{
				return this._user.Entity;
			}
			set
			{
				user previousValue = this._user.Entity;
				if (((previousValue != value) 
							|| (this._user.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._user.Entity = null;
						previousValue.friendships.Remove(this);
					}
					this._user.Entity = value;
					if ((value != null))
					{
						value.friendships.Add(this);
						this._user1 = value.ID;
					}
					else
					{
						this._user1 = default(int);
					}
					this.SendPropertyChanged("user");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_friendship1", Storage="_user3", ThisKey="user2", OtherKey="ID", IsForeignKey=true)]
		public user user3
		{
			get
			{
				return this._user3.Entity;
			}
			set
			{
				user previousValue = this._user3.Entity;
				if (((previousValue != value) 
							|| (this._user3.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._user3.Entity = null;
						previousValue.friendships1.Remove(this);
					}
					this._user3.Entity = value;
					if ((value != null))
					{
						value.friendships1.Add(this);
						this._user2 = value.ID;
					}
					else
					{
						this._user2 = default(int);
					}
					this.SendPropertyChanged("user3");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.user_game")]
	public partial class user_game : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _userID;
		
		private int _gameID;
		
		private System.Nullable<int> _numberOfPeels;
		
		private System.Nullable<bool> _performedFirstPeel;
		
		private System.Nullable<int> _avgTimeBetweenPeels;
		
		private System.Nullable<int> _numberOfDumps;
		
		private System.Nullable<int> _avgTimeBetweenDumps;
		
		private System.Nullable<bool> _isWinner;
		
		private EntityRef<game> _game;
		
		private EntityRef<user> _user;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnuserIDChanging(int value);
    partial void OnuserIDChanged();
    partial void OngameIDChanging(int value);
    partial void OngameIDChanged();
    partial void OnnumberOfPeelsChanging(System.Nullable<int> value);
    partial void OnnumberOfPeelsChanged();
    partial void OnperformedFirstPeelChanging(System.Nullable<bool> value);
    partial void OnperformedFirstPeelChanged();
    partial void OnavgTimeBetweenPeelsChanging(System.Nullable<int> value);
    partial void OnavgTimeBetweenPeelsChanged();
    partial void OnnumberOfDumpsChanging(System.Nullable<int> value);
    partial void OnnumberOfDumpsChanged();
    partial void OnavgTimeBetweenDumpsChanging(System.Nullable<int> value);
    partial void OnavgTimeBetweenDumpsChanged();
    partial void OnisWinnerChanging(System.Nullable<bool> value);
    partial void OnisWinnerChanged();
    #endregion
		
		public user_game()
		{
			this._game = default(EntityRef<game>);
			this._user = default(EntityRef<user>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_userID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int userID
		{
			get
			{
				return this._userID;
			}
			set
			{
				if ((this._userID != value))
				{
					if (this._user.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnuserIDChanging(value);
					this.SendPropertyChanging();
					this._userID = value;
					this.SendPropertyChanged("userID");
					this.OnuserIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_gameID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int gameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((this._gameID != value))
				{
					if (this._game.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OngameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("gameID");
					this.OngameIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_numberOfPeels", DbType="Int")]
		public System.Nullable<int> numberOfPeels
		{
			get
			{
				return this._numberOfPeels;
			}
			set
			{
				if ((this._numberOfPeels != value))
				{
					this.OnnumberOfPeelsChanging(value);
					this.SendPropertyChanging();
					this._numberOfPeels = value;
					this.SendPropertyChanged("numberOfPeels");
					this.OnnumberOfPeelsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_performedFirstPeel", DbType="Bit")]
		public System.Nullable<bool> performedFirstPeel
		{
			get
			{
				return this._performedFirstPeel;
			}
			set
			{
				if ((this._performedFirstPeel != value))
				{
					this.OnperformedFirstPeelChanging(value);
					this.SendPropertyChanging();
					this._performedFirstPeel = value;
					this.SendPropertyChanged("performedFirstPeel");
					this.OnperformedFirstPeelChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_avgTimeBetweenPeels", DbType="Int")]
		public System.Nullable<int> avgTimeBetweenPeels
		{
			get
			{
				return this._avgTimeBetweenPeels;
			}
			set
			{
				if ((this._avgTimeBetweenPeels != value))
				{
					this.OnavgTimeBetweenPeelsChanging(value);
					this.SendPropertyChanging();
					this._avgTimeBetweenPeels = value;
					this.SendPropertyChanged("avgTimeBetweenPeels");
					this.OnavgTimeBetweenPeelsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_numberOfDumps", DbType="Int")]
		public System.Nullable<int> numberOfDumps
		{
			get
			{
				return this._numberOfDumps;
			}
			set
			{
				if ((this._numberOfDumps != value))
				{
					this.OnnumberOfDumpsChanging(value);
					this.SendPropertyChanging();
					this._numberOfDumps = value;
					this.SendPropertyChanged("numberOfDumps");
					this.OnnumberOfDumpsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_avgTimeBetweenDumps", DbType="Int")]
		public System.Nullable<int> avgTimeBetweenDumps
		{
			get
			{
				return this._avgTimeBetweenDumps;
			}
			set
			{
				if ((this._avgTimeBetweenDumps != value))
				{
					this.OnavgTimeBetweenDumpsChanging(value);
					this.SendPropertyChanging();
					this._avgTimeBetweenDumps = value;
					this.SendPropertyChanged("avgTimeBetweenDumps");
					this.OnavgTimeBetweenDumpsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_isWinner", DbType="Bit")]
		public System.Nullable<bool> isWinner
		{
			get
			{
				return this._isWinner;
			}
			set
			{
				if ((this._isWinner != value))
				{
					this.OnisWinnerChanging(value);
					this.SendPropertyChanging();
					this._isWinner = value;
					this.SendPropertyChanged("isWinner");
					this.OnisWinnerChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="game_user_game", Storage="_game", ThisKey="gameID", OtherKey="gameID", IsForeignKey=true)]
		public game game
		{
			get
			{
				return this._game.Entity;
			}
			set
			{
				game previousValue = this._game.Entity;
				if (((previousValue != value) 
							|| (this._game.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._game.Entity = null;
						previousValue.user_games.Remove(this);
					}
					this._game.Entity = value;
					if ((value != null))
					{
						value.user_games.Add(this);
						this._gameID = value.gameID;
					}
					else
					{
						this._gameID = default(int);
					}
					this.SendPropertyChanged("game");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_user_game", Storage="_user", ThisKey="userID", OtherKey="ID", IsForeignKey=true)]
		public user user
		{
			get
			{
				return this._user.Entity;
			}
			set
			{
				user previousValue = this._user.Entity;
				if (((previousValue != value) 
							|| (this._user.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._user.Entity = null;
						previousValue.user_games.Remove(this);
					}
					this._user.Entity = value;
					if ((value != null))
					{
						value.user_games.Add(this);
						this._userID = value.ID;
					}
					else
					{
						this._userID = default(int);
					}
					this.SendPropertyChanged("user");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.game")]
	public partial class game : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _gameID;
		
		private System.DateTime _gameStartTime;
		
		private int _gameDuration;
		
		private int _winner;
		
		private System.Xml.Linq.XElement _gameDataFile;
		
		private EntitySet<user_game> _user_games;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OngameIDChanging(int value);
    partial void OngameIDChanged();
    partial void OngameStartTimeChanging(System.DateTime value);
    partial void OngameStartTimeChanged();
    partial void OngameDurationChanging(int value);
    partial void OngameDurationChanged();
    partial void OnwinnerChanging(int value);
    partial void OnwinnerChanged();
    partial void OngameDataFileChanging(System.Xml.Linq.XElement value);
    partial void OngameDataFileChanged();
    #endregion
		
		public game()
		{
			this._user_games = new EntitySet<user_game>(new Action<user_game>(this.attach_user_games), new Action<user_game>(this.detach_user_games));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_gameID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int gameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((this._gameID != value))
				{
					this.OngameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("gameID");
					this.OngameIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_gameStartTime", DbType="DateTime NOT NULL")]
		public System.DateTime gameStartTime
		{
			get
			{
				return this._gameStartTime;
			}
			set
			{
				if ((this._gameStartTime != value))
				{
					this.OngameStartTimeChanging(value);
					this.SendPropertyChanging();
					this._gameStartTime = value;
					this.SendPropertyChanged("gameStartTime");
					this.OngameStartTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_gameDuration", DbType="Int NOT NULL")]
		public int gameDuration
		{
			get
			{
				return this._gameDuration;
			}
			set
			{
				if ((this._gameDuration != value))
				{
					this.OngameDurationChanging(value);
					this.SendPropertyChanging();
					this._gameDuration = value;
					this.SendPropertyChanged("gameDuration");
					this.OngameDurationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_winner", DbType="Int NOT NULL")]
		public int winner
		{
			get
			{
				return this._winner;
			}
			set
			{
				if ((this._winner != value))
				{
					this.OnwinnerChanging(value);
					this.SendPropertyChanging();
					this._winner = value;
					this.SendPropertyChanged("winner");
					this.OnwinnerChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_gameDataFile", DbType="Xml", UpdateCheck=UpdateCheck.Never)]
		public System.Xml.Linq.XElement gameDataFile
		{
			get
			{
				return this._gameDataFile;
			}
			set
			{
				if ((this._gameDataFile != value))
				{
					this.OngameDataFileChanging(value);
					this.SendPropertyChanging();
					this._gameDataFile = value;
					this.SendPropertyChanged("gameDataFile");
					this.OngameDataFileChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="game_user_game", Storage="_user_games", ThisKey="gameID", OtherKey="gameID")]
		public EntitySet<user_game> user_games
		{
			get
			{
				return this._user_games;
			}
			set
			{
				this._user_games.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_user_games(user_game entity)
		{
			this.SendPropertyChanging();
			entity.game = this;
		}
		
		private void detach_user_games(user_game entity)
		{
			this.SendPropertyChanging();
			entity.game = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.[user]")]
	public partial class user : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private string _email;
		
		private string _alias;
		
		private System.Nullable<System.DateTime> _creation_time;
		
		private System.Nullable<System.DateTime> _last_seen_time;
		
		private System.Nullable<int> _ranking;
		
		private System.Nullable<int> _skill_level;
		
		private System.Nullable<long> _experience;
		
		private EntitySet<friendRequest> _friendRequests;
		
		private EntitySet<friendRequest> _friendRequests1;
		
		private EntitySet<friendship> _friendships;
		
		private EntitySet<friendship> _friendships1;
		
		private EntitySet<user_game> _user_games;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void OnemailChanging(string value);
    partial void OnemailChanged();
    partial void OnaliasChanging(string value);
    partial void OnaliasChanged();
    partial void Oncreation_timeChanging(System.Nullable<System.DateTime> value);
    partial void Oncreation_timeChanged();
    partial void Onlast_seen_timeChanging(System.Nullable<System.DateTime> value);
    partial void Onlast_seen_timeChanged();
    partial void OnrankingChanging(System.Nullable<int> value);
    partial void OnrankingChanged();
    partial void Onskill_levelChanging(System.Nullable<int> value);
    partial void Onskill_levelChanged();
    partial void OnexperienceChanging(System.Nullable<long> value);
    partial void OnexperienceChanged();
    #endregion
		
		public user()
		{
			this._friendRequests = new EntitySet<friendRequest>(new Action<friendRequest>(this.attach_friendRequests), new Action<friendRequest>(this.detach_friendRequests));
			this._friendRequests1 = new EntitySet<friendRequest>(new Action<friendRequest>(this.attach_friendRequests1), new Action<friendRequest>(this.detach_friendRequests1));
			this._friendships = new EntitySet<friendship>(new Action<friendship>(this.attach_friendships), new Action<friendship>(this.detach_friendships));
			this._friendships1 = new EntitySet<friendship>(new Action<friendship>(this.attach_friendships1), new Action<friendship>(this.detach_friendships1));
			this._user_games = new EntitySet<user_game>(new Action<user_game>(this.attach_user_games), new Action<user_game>(this.detach_user_games));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_email", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string email
		{
			get
			{
				return this._email;
			}
			set
			{
				if ((this._email != value))
				{
					this.OnemailChanging(value);
					this.SendPropertyChanging();
					this._email = value;
					this.SendPropertyChanged("email");
					this.OnemailChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_alias", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string alias
		{
			get
			{
				return this._alias;
			}
			set
			{
				if ((this._alias != value))
				{
					this.OnaliasChanging(value);
					this.SendPropertyChanging();
					this._alias = value;
					this.SendPropertyChanged("alias");
					this.OnaliasChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_creation_time", DbType="DateTime")]
		public System.Nullable<System.DateTime> creation_time
		{
			get
			{
				return this._creation_time;
			}
			set
			{
				if ((this._creation_time != value))
				{
					this.Oncreation_timeChanging(value);
					this.SendPropertyChanging();
					this._creation_time = value;
					this.SendPropertyChanged("creation_time");
					this.Oncreation_timeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_last_seen_time", DbType="DateTime")]
		public System.Nullable<System.DateTime> last_seen_time
		{
			get
			{
				return this._last_seen_time;
			}
			set
			{
				if ((this._last_seen_time != value))
				{
					this.Onlast_seen_timeChanging(value);
					this.SendPropertyChanging();
					this._last_seen_time = value;
					this.SendPropertyChanged("last_seen_time");
					this.Onlast_seen_timeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ranking", DbType="Int")]
		public System.Nullable<int> ranking
		{
			get
			{
				return this._ranking;
			}
			set
			{
				if ((this._ranking != value))
				{
					this.OnrankingChanging(value);
					this.SendPropertyChanging();
					this._ranking = value;
					this.SendPropertyChanged("ranking");
					this.OnrankingChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_skill_level", DbType="Int")]
		public System.Nullable<int> skill_level
		{
			get
			{
				return this._skill_level;
			}
			set
			{
				if ((this._skill_level != value))
				{
					this.Onskill_levelChanging(value);
					this.SendPropertyChanging();
					this._skill_level = value;
					this.SendPropertyChanged("skill_level");
					this.Onskill_levelChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_experience", DbType="BigInt")]
		public System.Nullable<long> experience
		{
			get
			{
				return this._experience;
			}
			set
			{
				if ((this._experience != value))
				{
					this.OnexperienceChanging(value);
					this.SendPropertyChanging();
					this._experience = value;
					this.SendPropertyChanged("experience");
					this.OnexperienceChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_friendRequest", Storage="_friendRequests", ThisKey="ID", OtherKey="from_user")]
		public EntitySet<friendRequest> friendRequests
		{
			get
			{
				return this._friendRequests;
			}
			set
			{
				this._friendRequests.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_friendRequest1", Storage="_friendRequests1", ThisKey="ID", OtherKey="to_user")]
		public EntitySet<friendRequest> friendRequests1
		{
			get
			{
				return this._friendRequests1;
			}
			set
			{
				this._friendRequests1.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_friendship", Storage="_friendships", ThisKey="ID", OtherKey="user1")]
		public EntitySet<friendship> friendships
		{
			get
			{
				return this._friendships;
			}
			set
			{
				this._friendships.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_friendship1", Storage="_friendships1", ThisKey="ID", OtherKey="user2")]
		public EntitySet<friendship> friendships1
		{
			get
			{
				return this._friendships1;
			}
			set
			{
				this._friendships1.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="user_user_game", Storage="_user_games", ThisKey="ID", OtherKey="userID")]
		public EntitySet<user_game> user_games
		{
			get
			{
				return this._user_games;
			}
			set
			{
				this._user_games.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_friendRequests(friendRequest entity)
		{
			this.SendPropertyChanging();
			entity.user = this;
		}
		
		private void detach_friendRequests(friendRequest entity)
		{
			this.SendPropertyChanging();
			entity.user = null;
		}
		
		private void attach_friendRequests1(friendRequest entity)
		{
			this.SendPropertyChanging();
			entity.user1 = this;
		}
		
		private void detach_friendRequests1(friendRequest entity)
		{
			this.SendPropertyChanging();
			entity.user1 = null;
		}
		
		private void attach_friendships(friendship entity)
		{
			this.SendPropertyChanging();
			entity.user = this;
		}
		
		private void detach_friendships(friendship entity)
		{
			this.SendPropertyChanging();
			entity.user = null;
		}
		
		private void attach_friendships1(friendship entity)
		{
			this.SendPropertyChanging();
			entity.user3 = this;
		}
		
		private void detach_friendships1(friendship entity)
		{
			this.SendPropertyChanging();
			entity.user3 = null;
		}
		
		private void attach_user_games(user_game entity)
		{
			this.SendPropertyChanging();
			entity.user = this;
		}
		
		private void detach_user_games(user_game entity)
		{
			this.SendPropertyChanging();
			entity.user = null;
		}
	}
}
#pragma warning restore 1591
