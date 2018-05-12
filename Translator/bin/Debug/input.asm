STACK_SEG SEGMENT STACK
	DW 512 DUP(?)
STACK_SEG ENDS

CODESC	SEGMENT 'CODE'
	ASSUME CS:CODESC, SS:STACK_SEG

BEGIN:	.386
call   scanf
mov    dword ptr [ebp+400], ECX
mov    eax, dword ptr [ebp+400]
mov    dword ptr [ebp+4], eax
mov    eax, dword ptr [ebp+4]
mov    ebx, 2
cdq
idiv   ebx
mov    dword ptr [ebp+404], eax
mov    eax, dword ptr [ebp+404]
mov    dword ptr [ebp+4], eax
inc    dword ptr [ebp+4]
mov    eax, dword ptr [ebp+4]
add    eax, 34
mov    dword ptr [ebp+408], eax
mov    eax, dword ptr [ebp+408]
mov    dword ptr [ebp+8], eax
mov    eax, dword ptr [ebp+8]
call   printf
MOV AH, 4CH
INT 21H
PRINTF PROC
	    MOV    EBX, EAX
	    AND    EBX, 80000000H
	    CMP    EBX, 0
	    JZ	   M0
	    MOV    ECX, EAX
	    MOV    AH,	2
	    MOV    DL, 2DH
	    INT    21H
	    MOV    EDX, 0
	    SUB    EDX, ECX
	    MOV    EAX, EDX
M0:    MOV ECX, 0
	    MOV EBX, 10
M1:    MOV EDX, 0
	    DIV EBX
	    PUSH DX
	    INC ECX
	    CMP EAX, 0
	    JNZ M1
M2:    POP DX
	    OR DL, 30H
	    MOV AH,2
	    INT 21h
	    LOOP M2
;print new line (LF, CR)
   MOV DL, 0AH
	MOV AH,2
	INT 21h
	MOV DL, 0DH
	MOV AH,2
	INT 21h
	RET
PRINTF ENDP
SCANF  PROC
MOV SI, 0	;флаг дали числото е отрицателно
    MOV EBX, 10
    MOV ECX, 0
M21:MOV AH, 0
    ;прочита се число от клавиатурата
    PUSH EBX
    MOV BH, AH
    MOV AH, 1
    INT 21H
    MOV AH, BH
    POP EBX
    ;край на четенето от клавиатурата
    CMP AL, 0DH	;дали е натиснат CR?
    JE M22
    ;проверка за въведен знак -
    CMP AL, 2DH
    JNE M20
    MOV SI, 1	;числото е отрицателно
    JMP M21
M20:SUB AL, 30H	;преобразуване на ASCII цифрата в двоична
    PUSH EAX	;съхраняване на последното въвеждане
    MOV EAX, ECX
    MUL EBX	;умножаване на десетичната сума с 10
    POP ECX	;последното въвеждане се извлича обратно
    ADD ECX, EAX	;в CX и се сумира
    JMP M21
    ;проверка дали числото е отрицателно (със знак - отпред)
M22:CMP SI, 1
    JNE M23
    NEG ECX
;print new line (LF, CR)
M23:MOV DL, 0AH
	MOV AH,2
	INT 21h
	MOV DL, 0DH
	MOV AH,2
	INT 21h
	RET
SCANF  ENDP
CODESC	ENDS
   END BEGIN
