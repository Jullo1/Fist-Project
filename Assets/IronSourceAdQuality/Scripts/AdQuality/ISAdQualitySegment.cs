using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ISAdQualitySegment
{
	public string name = null;
	public int age;
	public string gender = null;
	public int level;
	public int isPaying;
	public double inAppPurchasesTotal;
	public long userCreationDate;
	public Dictionary<string,string> customs;

	public ISAdQualitySegment ()
	{
		age = -1;
		level = -1;
		isPaying = -1;
		inAppPurchasesTotal = -1;
		userCreationDate = -1;
		customs = new Dictionary<string,string> ();
	}

	public void setCustom(string key, string value){
		customs.Add (key, value);
	}

	public Dictionary<string,string> getSegmentAsDict ()
	{
		Dictionary<string,string> dict = new Dictionary<string,string> ();
		if (!string.IsNullOrEmpty(name)) 
		{
			dict.Add ("name", name);
		}
		if (age != -1) 
		{
			dict.Add ("age", age + "");
		}
		if (!string.IsNullOrEmpty(gender)) 
		{
			dict.Add ("gender", gender);
		}
		if (level != -1) 
		{
			dict.Add ("level", level + "");
		}
		if (isPaying > -1 && isPaying < 2) 
		{
			dict.Add ("isPaying", isPaying + "");
		}
		if (inAppPurchasesTotal > -1) 
		{
			dict.Add ("iapt", inAppPurchasesTotal + "");
		}
		if (userCreationDate != -1) 
		{
			dict.Add ("userCreationDate", userCreationDate + "");
		}
		Dictionary<string,string> result = dict.Concat(customs).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
		return result;
	}

}
