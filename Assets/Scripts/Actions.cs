using System;
using System.Collections.Generic;

public class Actions
{
    public List<Tuple<Increments, AgentDirections>> list = new(9);
    public Actions()
	{
        list.Add(new(Increments.minus, AgentDirections.forward));
        list.Add(new(Increments.minus, AgentDirections.left));
        list.Add(new(Increments.minus, AgentDirections.right));

        list.Add(new(Increments.zero, AgentDirections.forward));
        list.Add(new(Increments.zero, AgentDirections.left));
        list.Add(new(Increments.zero, AgentDirections.right));

        list.Add(new(Increments.plus, AgentDirections.forward));
        list.Add(new(Increments.plus, AgentDirections.left));
        list.Add(new(Increments.plus, AgentDirections.right));
    }
}

