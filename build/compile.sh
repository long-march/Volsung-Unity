echo "Compiling Volsung"
g++ -c -I ./../Volsung/include/ ../Volsung/src/*.cc -fPIC -std=c++17 -O3

echo "Compiling Interface"
g++ -c -I ./../Volsung/include/ VolsungCInterface.cc -fPIC -std=c++17 -O3

echo "Linking into shared library"
g++ --shared *.o -o libVolsungShared
