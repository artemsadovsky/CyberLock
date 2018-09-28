// This is the main DLL file.
#include "stdafx.h"
#include "CppGraphics.h"

CppGraphics::Drawer::Drawer()
{
	number = 6;
	frLeft=300;     // Левый отступ фрейма от границы окна-родителя
	frTop=300;      // Отступ сверху от границы окна-родителя
	frWidth=200;    // Ширина фрейма
	frHeight=200;   // Высота фрейма
}

CppGraphics::Drawer::~Drawer()
{
	
}

int CppGraphics::Drawer::Get(void)
{
	return number;
}

void CppGraphics::Drawer::Draw(void)
{

}