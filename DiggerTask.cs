using System;
using System.Drawing;
using Avalonia.Input;
using Digger.Architecture;

namespace Digger;

public enum Priority
{
    Sack = 4,
    Monster = 3,
    Digger = 2,
    Gold = 1,
    Terrain = 0,  
}

class Terrain : ICreature
{
    public CreatureCommand Act(int x, int y)
    {
        return new CreatureCommand()
        {
            DeltaX = 0,
            DeltaY = 0,
            TransformTo = null
        };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return true;
    }

    public int GetDrawingPriority()
    {
        return (int)Priority.Terrain;
    }

    public string GetImageFileName()
    {
        return "Terrain.png";
    }
}

class Player : ICreature
{
    public CreatureCommand Act(int x, int y)
    {
        var deltaX = 0;
        var deltaY = 0;

        if (Game.KeyPressed == Key.Left && x - 1 >= 0 && Game.Map[x - 1, y] is not Sack)
            deltaX = -1;

        else if (Game.KeyPressed == Key.Right && x + 1 < Game.MapWidth && Game.Map[x + 1, y] is not Sack)
            deltaX = 1;

        else if (Game.KeyPressed == Key.Up && y - 1 >= 0 && Game.Map[x, y - 1] is not Sack)
            deltaY = -1;

        else if (Game.KeyPressed == Key.Down && y + 1 < Game.MapHeight && Game.Map[x, y + 1] is not Sack)
            deltaY = 1;

        return new CreatureCommand()
        {
            DeltaX = deltaX,
            DeltaY = deltaY,
            TransformTo = null
        };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        if (conflictedObject is Gold)
            Game.Scores += 10;

        return GetDrawingPriority() < conflictedObject.GetDrawingPriority();
    }

    public int GetDrawingPriority()
    {
        return (int)Priority.Digger;
    }

    public string GetImageFileName()
    {
        return "Digger.png";
    }
}

class Sack : ICreature
{
    private int counter;
    public CreatureCommand Act(int x, int y)
    {
        if (y + 1 < Game.MapHeight)
        {
            var onNextStep = Game.Map[x, y + 1];

            if (onNextStep is null || (counter > 0 && onNextStep is Player) || (counter > 0 && onNextStep is Monster))
            {
                counter++;
                return new CreatureCommand() { DeltaX = 0, DeltaY = 1 };
            }
        }

        if (counter > 1) 
        {
            counter = 0;
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0, TransformTo = new Gold() };
        }

        counter = 0;
        return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return GetDrawingPriority() < conflictedObject.GetDrawingPriority();
    }

    public int GetDrawingPriority()
    {
        return (int)Priority.Sack;
    }

    public string GetImageFileName()
    {
        return "Sack.png";
    }
}

class Gold : ICreature
{
    public CreatureCommand Act(int x, int y)
    {
        return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return GetDrawingPriority() < conflictedObject.GetDrawingPriority();
    }

    public int GetDrawingPriority()
    {
        return (int)Priority.Gold;
    }

    public string GetImageFileName()
    {
        return "Gold.png";
    }
}

class Monster : ICreature
{
    public CreatureCommand Act(int x, int y)
    {
        if (PlayerIsOnMap() == null)
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };

        var playerLocationPoint = PlayerIsOnMap();
        var playerX = ((Point)playerLocationPoint).X;
        var playerY = ((Point)playerLocationPoint).Y;

        if (playerX - x != 0 || playerY - y != 0)
        {    
            if (playerX - x > 0 && CanMakeStep(Game.Map[x + 1, y]))
                return new CreatureCommand() { DeltaX = 1, DeltaY = 0 };

            if (playerX - x < 0 && CanMakeStep(Game.Map[x - 1, y]))
                return new CreatureCommand() { DeltaX = -1, DeltaY = 0 };

            if (playerY - y > 0 && CanMakeStep(Game.Map[x, y + 1]))
                return new CreatureCommand() { DeltaX = 0, DeltaY = 1 };

            if (playerY - y < 0 && CanMakeStep(Game.Map[x, y - 1]))
                return new CreatureCommand() { DeltaX = 0, DeltaY = -1 };
        }

        return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
    }

    private bool CanMakeStep(ICreature creature)
    {
        if (creature is Monster || creature is Terrain || creature is Sack)
            return false;

        return true;
    }

    private Point? PlayerIsOnMap()
    {
        for (int i = 0; i < Game.MapWidth; i++)
            for (int j = 0; j < Game.MapHeight; j++)
                if (Game.Map[i, j] is Player)
                    return new Point(i, j);

        return null;
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return GetDrawingPriority() <= conflictedObject.GetDrawingPriority();
    }

    public int GetDrawingPriority()
    {
        return (int)Priority.Monster;
    }

    public string GetImageFileName()
    {
        return "Monster.png";
    }
}