// This is the main DLL file.
#include "CppGraphics.h"

CppGraphics::Drawer::Drawer()
{
	number = 6;
}

CppGraphics::Drawer::~Drawer()
{
	
}

int CppGraphics::Drawer::Get(void)
{
	return number;
}