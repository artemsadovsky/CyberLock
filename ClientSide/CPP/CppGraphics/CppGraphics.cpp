// This is the main DLL file.
#include "stdafx.h"
#include "CppGraphics.h"

CppGraphics::Drawer::Drawer()
{
	number = 6;
	frLeft=300;     // ����� ������ ������ �� ������� ����-��������
	frTop=300;      // ������ ������ �� ������� ����-��������
	frWidth=200;    // ������ ������
	frHeight=200;   // ������ ������
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