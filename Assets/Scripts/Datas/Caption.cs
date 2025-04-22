using System.Collections.Generic;
public class Caption{
 	 public int id;
 	 public string text;
 	 public string name;
 	 public int type;
 	 public int feel;
 	 public int money;
 	 public int fans;
 	 public float chance;
 	 public string chatbubble;
 	 public string gift;
 	 public string sound;
 	 public string cat;
}
public class CaptionModel : IModel{
public List<Caption> values;
}