#include <stdio.h>

int fib_iter(int a, int b, int n)
{
	if (n == 0) {
		return b;
	} else {
		return fib_iter(a + b, a, n-1);
	}
}

int main(int argc, char* argv[])
{
	int i;
	i = fib_iter(5, 5, 5);
	printf("%d\n", i);
	return 0;
}
