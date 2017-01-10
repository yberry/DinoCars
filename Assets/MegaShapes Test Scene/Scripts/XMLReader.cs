
#if false
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class XMLValue
{
	public string	name;
	public string	value;
}

public class XMLNode
{
	public String                           tagName;
	public XMLNode                          parentNode;
	public ArrayList                        children;
	public Dictionary<String, String>       attributes;
	public List<XMLValue>					values;

	public XMLNode()
	{
		tagName = "NONE";
		parentNode = null;
		children = new ArrayList();
		attributes = new Dictionary<String, String>();
		values = new List<XMLValue>();
	}
}

public class XMLReader
{
	private static char TAG_START = '<';
	private static char TAG_END = '>';
	private static char SPACE = ' ';
	private static char QUOTE = '"';
	private static char SLASH = '/';
	private static char EQUALS = '=';
	private static String BEGIN_QUOTE = "" + EQUALS + QUOTE;

	public XMLReader()
	{
	}

	public XMLNode read(String xml)
	{
		int index = 0;
		int lastIndex = 0;
		XMLNode rootNode = new XMLNode();
		XMLNode currentNode = rootNode;

		while ( true )
		{
			index = xml.IndexOf(TAG_START, lastIndex);

			if ( index < 0 || index >= xml.Length )
			{
				break;
			}

			index++;

			lastIndex = xml.IndexOf(TAG_END, index);
			if ( lastIndex < 0 || lastIndex >= xml.Length )
			{
				break;
			}

			int tagLength = lastIndex - index;
			String xmlTag = xml.Substring(index, tagLength);

			// if the tag starts with a </ then it is an end tag
			//
			if ( xmlTag[0] == SLASH )
			{
				currentNode = currentNode.parentNode;
				continue;
			}

			bool openTag = true;

			// if the tag ends in /> the tag can be considered closed
			if ( xmlTag[tagLength - 1] == SLASH )
			{
				// cut away the slash
				xmlTag = xmlTag.Substring(0, tagLength - 1);
				openTag = false;
			}


			XMLNode node = parseTag(xmlTag);
			node.parentNode = currentNode;
			currentNode.children.Add(node);

			if ( openTag )
			{
				currentNode = node;
			}

		}

		return rootNode;
	}


	public XMLNode parseTag(String xmlTag)
	{
		XMLNode node = new XMLNode();

		int nameEnd = xmlTag.IndexOf(SPACE, 0);
		if ( nameEnd < 0 )
		{
			node.tagName = xmlTag;
			return node;
		}

		String tagName = xmlTag.Substring(0, nameEnd);
		node.tagName = tagName;

		String attrString = xmlTag.Substring(nameEnd, xmlTag.Length - nameEnd);
		return parseAttributes(attrString, node);
	}

	public XMLNode parseAttributes(String xmlTag, XMLNode node)
	{
		int index = 0;
		int attrNameIndex = 0;
		int lastIndex = 0;

		while ( true )
		{
			index = xmlTag.IndexOf(BEGIN_QUOTE, lastIndex);
			if ( index < 0 || index > xmlTag.Length )
			{
				break;
			}

			attrNameIndex = xmlTag.LastIndexOf(SPACE, index);
			if ( attrNameIndex < 0 || attrNameIndex > xmlTag.Length )
			{
				break;
			}


			attrNameIndex++;
			String attrName = xmlTag.Substring(attrNameIndex, index - attrNameIndex);

			// skip the equal and quote character
			//
			index += 2;

			lastIndex = xmlTag.IndexOf(QUOTE, index);
			if ( lastIndex < 0 || lastIndex > xmlTag.Length )
			{
				break;
			}

			int tagLength = lastIndex - index;
			String attrValue = xmlTag.Substring(index, tagLength);

			node.attributes[attrName] = attrValue;

			XMLValue val = new XMLValue();
			val.name = attrName;
			val.value = attrValue;
			node.values.Add(val);
		}

		return node;
	}

	public void printXML(XMLNode node, int indent)
	{
		indent++;

		foreach ( XMLNode n in node.children )
		{
			String attr = " ";
			foreach ( KeyValuePair<String, String> p in n.attributes )
			{
				attr += "[" + p.Key + ": " + p.Value + "] ";
				//Debug.Log( attr );
			}

			String indentString = "";
			for ( int i = 0; i < indent; i++ )
			{
				indentString += "-";
			}

			Debug.Log("" + indentString + " " + n.tagName + attr);
			printXML(n, indent);
		}
	}
}
#endif