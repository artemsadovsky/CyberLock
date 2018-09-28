// CppGraphics.h

#pragma once

using namespace System;

namespace CppGraphics {

	public ref class Drawer
	{
	public:
		Drawer();
		~Drawer();
		int Get(void);
	private:
		int number;
	};
}
