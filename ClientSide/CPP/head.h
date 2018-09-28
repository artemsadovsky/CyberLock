#pragma once

using namespace System;

namespace Graphics{
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