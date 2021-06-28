# CppMemoryVisualizer

![](/snapshot.png)

GNU Debugger(GDB)를 이용한 C++ 메모리 시각화 프로그램입니다. 디버깅과 중단점 설정이 가능하며, 스택과 힙 메모리를 표로 시각화된 모습을 볼 수 있습니다.

## Compatibility
* Windows 10 (64-bit)

## Dependencies
* ### [MSYS2](https://www.msys2.org/)
   * 공식 사이트의 다운로드 속도가 느리면 [이곳](https://github.com/msys2/msys2-installer/releases)에서 최신 릴리즈를 받으세요.
   * 설치 방법: [이곳](https://stackoverflow.com/questions/30069830/how-to-install-mingw-w64-and-msys2/30071634#30071634)를 눌러 1~2단계를 참고하세요.

* ### [GCC](https://gcc.gnu.org/) (Rev6, Built by MSYS2 project, v10.3.0)
  * `$ pacman -S mingw-w64-i686-gcc`

* ### [GDB](https://www.gnu.org/software/gdb/) (v10.2)
  * `$ pacman -S mingw-w64-i686-toolchain`

## Nuget Package
* ### [AvalonEdit](http://avalonedit.net/) (v6.1.2.30)
