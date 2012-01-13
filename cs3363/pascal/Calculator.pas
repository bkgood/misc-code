{
  William Good
  October 20, 2011
  CS 3363
  Programming assignment 1
}

Program Calculator;
Const
    { Number of elements a stack can hold }
    StackSize = 255;
Type
    { Define a new stack data type }
    Stack = Record
        { Stack contents }
        Data: Array[1..StackSize] of Integer;
        { Index of topmost element of stack }
        Tip: Integer;
    End;

{ Returns a new stack }
Function StackInit(): Stack;
Var
    I: Integer;
Begin
    With StackInit Do Begin
        For I := 1 To StackSize Do Begin
            Data[I] := 0;
        End;
        Tip := 0;
    End;
End;

{ Pushes an integer onto a stack }
Procedure StackPush(Var S: Stack; I: Integer);
Begin
    With S Do Begin
        Tip := Tip + 1;
        Data[Tip] := I;
    End;
End;

{ Pops an integer from a stack }
Function StackPop(Var S: Stack): Integer;
Begin
    With S Do Begin
        StackPop := Data[Tip];
        Tip := Tip - 1;
    End;
End;

{ Performs operation associated with operator Op on stack S.
  If Op is unrecognized, nothing is performed. }
Procedure RunOperator(Var S: Stack; Op: Char);
Var
    Temp: Integer;
Begin
    Case Op Of
    '+':
        Begin
        StackPush(S, StackPop(S) + StackPop(S));
        End;
    '*':
        Begin
        StackPush(S, StackPop(S) * StackPop(S));
        End;
    '-':
        Begin
        Temp := StackPop(S);
        StackPush(S, StackPop(S) - Temp);
        End;
    '/':
        Begin
        Temp := StackPop(S);
        StackPush(S, StackPop(S) Div Temp);
        End;
    End;
End;

{ Evaluates a line of postfix calculator input and returns the result (top of
  stack). }
Function Eval(Expr: String): Integer;
Var
    C: Char; { C holds the character from Expr currently being processed }
    X, I: Integer; { Inputed numbers are parsed into X, I is a loop counter }
    InNumber: Boolean; { Used to determine if we're parsing a number }
    S: Stack; { Our stack }
Begin
    X := 0;
    InNumber := False;
    S := StackInit();
    For I := 1 To Length(Expr) Do Begin
        C := Expr[I];
        If ((C >= '0') and (C <= '9')) Then Begin
            X := 10 * X + (Ord(C) - 48);
            InNumber := True;
        End Else Begin
            { If we were parsing a number and now we're not, push the parsed
              number on to the stack. }
            If InNumber Then Begin
                StackPush(S, X);
                X := 0;
            End;
            InNumber := False;
            { Calling RunOperator indescriminately here, works because }
            { unrecognized input is ignored. }
            RunOperator(S, C);
        End;
    End;
    Eval := StackPop(S);
End;

Var
    L: String; { line from stdin currently being processed }
Begin
    While Not EOLN Do Begin
        ReadLn(L);
        Write(L);
        Write(' = ');
        WriteLn(Eval(L));
    End;
End.
