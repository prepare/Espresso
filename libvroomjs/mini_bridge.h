//MIT 2015, WinterDev
#pragma once

#include <v8.h>
#include <stdlib.h>
#include <stdint.h>
#include <iostream>

using namespace v8;




class ExternalManagedHandler
{
public:
	 
	int managedIndex;
	v8::Persistent<v8::Object> v8InstanceHandler;
	ExternalManagedHandler(int mIndex);
};

class ExternalTypeMember
{

public:
	int managedIndex; 
	int memberkind; 
};



class BinaryStreamReader
{

public:
	const char* stream;
	int length;
	int pos;

	BinaryStreamReader(const char* stream,int length);
	int ReadInt16();
	int ReadInt32();
	std::wstring ReadUtf16String();
};

class ExternalTypeDefinition 
{

public:		
	int managedIndex; 
	int memberkind; 
	v8::Handle<v8::ObjectTemplate> handlerToJsObjectTemplate;
	ExternalTypeDefinition(int mIndex);
	void ReadTypeDefinitionFromStream(BinaryStreamReader* reader); 
};