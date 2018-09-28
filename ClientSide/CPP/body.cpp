#include "Stdafx.h"
#include "head.h"

Graphics::Drawer::Drawer()
{
	number = 6;
}

Graphics::Drawer::~Drawer()
{
	
}

int Graphics::Drawer::Get(void)
{
	return number;
}