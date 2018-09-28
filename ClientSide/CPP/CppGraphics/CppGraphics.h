// CppGraphics.h

#pragma once
// Windows

using namespace System;

namespace CppGraphics {

	public ref class Drawer
	{
	public:
		Drawer();
		~Drawer();
		int Get(void);
		void Draw(void);
	private:
		int number;
		//HWND win_pw;        // ����� ���� PW
		//HWND frame;         // ����� ������ ������
		int frLeft;     // ����� ������ ������ �� ������� ����-��������
		int frTop;      // ������ ������ �� ������� ����-��������
		int frWidth;    // ������ ������
		int frHeight;   // ������ ������
	};
}
