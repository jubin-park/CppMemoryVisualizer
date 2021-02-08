# CppMemoryVisualizer

<p align="center">
  <img src="https://github.com/jubin-park/CppMemoryVisualizer/blob/main/snapshot.png?raw=true" alt="CppMemoryVisualizer image"/>
</p>

GNU Debugger(GDB)를 이용한 C++ 메모리 시각화 프로그램입니다. 디버깅과 중단점 설정이 가능하며, 스택과 힙 메모리를 표로 시각화된 모습을 볼 수 있습니다.

## Compatibility
* Windows 10
* MSYS2 **32-bit** (NOT 64-bit)

## Dependencies
* ### [MSYS2](https://www.msys2.org/) (v2021.01.05)
   * 공식 사이트의 다운로드 속도가 느리면 [이곳](https://github.com/msys2/msys2-installer/releases)에서 최신 릴리즈를 받으세요.
   * 설치 방법: [이곳](https://stackoverflow.com/questions/30069830/how-to-install-mingw-w64-and-msys2/30071634#30071634)를 눌러 1~2단계를 참고하세요.

* ### [GCC](https://gcc.gnu.org/) (Rev6, Built by MSYS2 project, 10.2.0)
  * `$ pacman -S mingw-w64-i686-gcc`

* ### [GDB](https://www.gnu.org/software/gdb/) (v10.1)
  * `$ pacman -S mingw-w64-i686-toolchain`

## Nuget Package
* ### [AvalonEdit](http://avalonedit.net/) (v6.0.1)

## Unsupported Features
*Announcement*: 아래 사항들은 업데이트 이후 변경될 수 있습니다.

타입 파싱코드가 완벽하지 않습니다. 따라서 어떤 변수는 화면에 그려지지 않거나 생략될 수 있습니다.

### **Cannot Visualize**
* Heap of `std::string`
* Member `std::string` of `struct|class`
* `std::vector`
* Member `struct|class` Array of `struct|class`

## Why GDB, not WinDbg?
본래 이 프로젝트는 Windows 전용 디버거인 WinDbg를 이용하여 개발을 시작하였습니다. 그러나 아래와 같은 문제로 GDB로 변경하였고 문제는 해결됐습니다.

* 디버그 레벨에서 스택프레임의 로컬 변수가 선언 순서대로 나열되지 않는 현상
* 레퍼런스 타입(`&`)을 포인터로 변환하여 인식하는 현상
