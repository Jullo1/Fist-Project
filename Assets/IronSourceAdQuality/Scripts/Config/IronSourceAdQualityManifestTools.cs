using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Collections.Generic;

public class IronSourceAdQualityManifestTools
{
#if UNITY_EDITOR
static string outputFile = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
	public static void GenerateManifest()
	{
		// only copy over a fresh copy of the AndroidManifest if one does not exist
		if (!File.Exists(outputFile))
		{
#if UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
	var inputFile = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/androidplayer/AndroidManifest.xml");
#elif UNITY_5_2
	var inputFile = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/androidplayer/Apk/AndroidManifest.xml");
#else				
	var inputFile = Path.Combine(EditorApplication.applicationPath, "../PlaybackEngines/androidplayer/Apk/AndroidManifest.xml");
#endif
	File.Copy(inputFile, outputFile);
		}
		UpdateManifest();
	}

	private static string _namespace = "";
	private static XmlDocument _document = null;
	private static XmlNode _manifestNode = null;
	private static XmlNode _applicationNode = null;

	private static void LoadManifest(){
		_document = new XmlDocument();
		_document.Load(outputFile);

		if (_document == null)
		{
			Debug.LogError("Couldn't load " + outputFile);
			return;
		}

		_manifestNode = FindChildNode(_document, "manifest");
		_namespace = _manifestNode.GetNamespaceOfPrefix("android");
		_applicationNode = FindChildNode(_manifestNode, "application");

		if (_applicationNode == null) {
			Debug.LogError("Error parsing " + outputFile);
			return;
		}
	}

	private static void SaveManifest(){
		_document.Save(outputFile);
	}

	public static void UpdateManifest() {
		LoadManifest ();

		SetPermission("android.permission.INTERNET");
		SetPermission("android.permission.ACCESS_NETWORK_STATE");

		SaveManifest ();
	}

	public static void AddActivity(string activityName, Dictionary<string, string> attributes) {
		AppendApplicationElement("activity", activityName, attributes);
	}

	public static void RemoveActivity(string activityName) {
		RemoveApplicationElement("activity", activityName);
	}

	public static void SetPermission(string permissionName) {
		PrependManifestElement("uses-permission", permissionName);
	}

	public static void RemovePermission(string permissionName) {
		RemoveManifestElement("uses-permission", permissionName);
	}

	public static XmlElement AppendApplicationElement(string tagName, string name, Dictionary<string, string> attributes) {
		return AppendElementIfMissing(tagName, name, attributes, _applicationNode);
	}

	public static void RemoveApplicationElement(string tagName, string name) {
		RemoveElement(tagName, name, _applicationNode);
	}

	public static XmlElement PrependManifestElement(string tagName, string name) {
		return PrependElementIfMissing(tagName, name, null, _manifestNode);
	}

	public static void RemoveManifestElement(string tagName, string name) {
		RemoveElement(tagName, name, _manifestNode);
	}

	public static XmlElement AddMetaDataTag(string mdName, string mdValue) {
		return AppendApplicationElement("meta-data", mdName, new Dictionary<string, string>() {
			{ "value", mdValue }
		});
	}

	public static XmlElement AppendElementIfMissing(string tagName, string name, Dictionary<string, string> otherAttributes, XmlNode parent) {
		XmlElement e = null;
		if (!string.IsNullOrEmpty(name)) {
			e = FindElementWithTagAndName(tagName, name, parent);
		}

		if (e == null)
		{
			e = _document.CreateElement(tagName);
			if (!string.IsNullOrEmpty(name)) {
				e.SetAttribute("name", _namespace, name);
			}

			parent.AppendChild(e);
		}

		if (otherAttributes != null) {
			foreach(string key in otherAttributes.Keys) {
				e.SetAttribute(key, _namespace, otherAttributes[key]);
			}
		}

		return e;
	}

	public static XmlElement PrependElementIfMissing(string tagName, string name, Dictionary<string, string> otherAttributes, XmlNode parent) {
		XmlElement e = null;
		if (!string.IsNullOrEmpty(name)) {
			e = FindElementWithTagAndName(tagName, name, parent);
		}

		if (e == null)
		{
			e = _document.CreateElement(tagName);
			if (!string.IsNullOrEmpty(name)) {
				e.SetAttribute("name", _namespace, name);
			}

			parent.PrependChild(e);
		}

		if (otherAttributes != null) {
			foreach(string key in otherAttributes.Keys) {
				e.SetAttribute(key, _namespace, otherAttributes[key]);
			}
		}

		return e;
	}

	public static void RemoveElement(string tagName, string name, XmlNode parent) {
		XmlElement e = FindElementWithTagAndName(tagName, name, parent);
		if (e != null)
		{
			parent.RemoveChild(e);
		}
	}

	public static XmlNode FindChildNode(XmlNode parent, string tagName)
	{
		XmlNode curr = parent.FirstChild;
		while (curr != null)
		{
			if (curr.Name.Equals(tagName))
			{
				return curr;
			}
			curr = curr.NextSibling;
		}
		return null;
	}

	public static XmlElement FindChildElement(XmlNode parent, string tagName)
	{
		XmlNode curr = parent.FirstChild;
		while (curr != null)
		{
			if (curr.Name.Equals(tagName))
			{
				return curr as XmlElement;
			}
			curr = curr.NextSibling;
		}
		return null;
	}

	public static XmlElement FindElementWithTagAndName(string tagName, string name, XmlNode parent)
	{
		var curr = parent.FirstChild;
		while (curr != null)
		{
			if (curr.Name.Equals(tagName) && curr is XmlElement && ((XmlElement)curr).GetAttribute("name", _namespace) == name)
			{
				return curr as XmlElement;
			}
			curr = curr.NextSibling;
		}
		return null;
	}
#endif
}