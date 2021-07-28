hirom
check bankcross off	


	org $C00000
	incbin "MM7.sfc"
	
slideF 		= $5F0
moon		= $5F1
newlevel 	= $5F2
newlevelF	= $5F3
immuneF	= $5F4
WeaponL	= $5F6
NewWeaponF	= $5F7
NewWeapon	= $5F8
CNT_SWP	= $5F9
CNT_INV	= $5FB
OHKO_F		= $5FD
INV_F		= $5E5
jumpH		= $5E6
DED_F		= $5E8

	org $C00C24
	db $9C	;Remove Intro

	org $C00066
	jsr SetMoonWalkVar
	
	org $C049EB
	jmp WeaponLockCheck2
	
	org $C00435
	jsr ControllerReverseC
	
	org $C00E18
	jsr AssignDeadF
	
	org $C00E6E
	jmp NewLevelCheck
	
	org $C0444E
	jsl CNT_INV_Preserve
	
	
	org $C04B3A
	jsl CNT_INV_Restorte
	
	org $C07B96
WeaponLockCheck2:
	ldx.w WeaponL
	beq .S
	jmp $49EE
.S
	sta $BC7
	jmp $49EE
NewLevelCheck:
	lda.w newlevelF
	beq .S
	stz.w newlevelF
	stz.w $B75
	stz $B74	;Checkpoint
	lda.w newlevel
	sta $B73	;STAGEID
	lda #$6
	sta.w $DF
	stz.w $E0
	lda.b #$F
	sta.w $AD
	jsr $39A0 	;Fadeout
	lda.b #$1
	xba
	lda.b #$3F
	tcs
	;Delete Threads
	stz.b $40
	stz.b $50
	stz.b $60
	stz.b $70
	stz.b $80
	jmp $BCA
.S
	lda.w OHKO_F
	beq .S2
	bpl .SC
	lda.b #$16
	sta.w OHKO_F
	jsl $C03205 ;Play Sound
	stz.w DED_F
.SC
	lda.w $C2E
	beq .S2
	lda.b #$1
	sta.w $C2E
.S2
	lda #$0
	rts
SetMoonWalkVar:
	ldx.w #1
	stx.w moon
	ldx.w #$5F4
	stx.w jumpH
	ldx.w #$2FF
	rts
ObjectImmuneCheck:
	lda.w immuneF
	beq .S
	clc
	rtl
.S
	jml $C30371
MegaManVarAssign:
	lda.b #3
	jsl $C03205 ;Cancel Charge Sound
	ldx.w NewWeapon
	lda $A00A,X
	bmi .S
	php
	tay
	jsl $C03A3A	;Slash Claw Tile Swap
	plp
.S
	lda.w NewWeapon
	cmp.w $BC7
	beq .C
	jsl $C04BA3 ;CLS Weapons
.C
	lda.w NewWeapon
	sta.w $BC7
	ldx.w $BC7
	cpx.b #$9
	bne .S2
	lda.b #1
	sta $80
	inc
	sta $8A
	lda $14
	and.b #$30
	sta $94
.S2
	jml Leave
WeaponLockSpawn:
	lda.w WeaponL
	beq .S
	ldx.w NewWeapon
	stx.w $BC7
	cpx.b #$9
	bne .N
	lda.b #1
	sta $C80
	inc
	sta $C8A
	lda $C14
	and.b #$30
	sta $C94
.N
	jsl SetupMegaWeaponPAL
	bra .C
.S
	stz.w $BC7
.C
	lda.w  OHKO_F
	beq .S2
	stz $C2E
	inc $C2E
.S2
	lda.b #$9C
	jml ReturnBack
AssignDeadF:
	lda.b #1
	sta.w DED_F
	lda.w $B81
	rts
ControllerReverseC:
	stx.b $A1
	lda.w CNT_SWP
	beq .S
	txa
	; B&Y => LEFT&DOWN
	and.w #$C000
	lsr : lsr : lsr : lsr : lsr
	sta.b $A1
	; A => RIGHT
	txa
	and.w #$80
	asl a
	ora.b $A1
	sta.b $A1
	; X => UP
	txa
	and.w #$40
	xba
	lsr #3
	ora.b $A1
	sta.b $A1
	; LEFT&DOWN => B&Y
	txa
	and.w #$600
	asl : asl : asl : asl : asl
	ora.b $A1
	sta.b $A1
	; RIGHT => A
	txa 
	and.w #$100
	lsr
	ora.b $A1
	sta.b $A1
	; UP => X
	txa 
	and.w #$800
	xba
	asl #3
	ora.b $A1
	sta.b $A1
	; Start & Select
	txa
	and.w #$3000
	ora.b $A1
	sta.b $A1
	ldx.b $A1

.S
	lda.w CNT_INV
	bne .GO
	ldx $A1
	txa
	rts
.GO
	txa
	; Y => A
	and.w #$4000
	xba
	asl a
	sta.b $A1
	; A => Y
	txa
	and.w #$80
	xba
	lsr a
	ora.b $A1
	sta.b $A1
	; X => B
	txa
	and.w #$40
	xba
	asl a
	ora.b $A1
	sta.b $A1
	; B => X
	txa
	and.w #$8000
	xba
	lsr a
	ora.b $A1
	sta.b $A1
	; Select => Start
	txa
	and.w #$2000
	lsr a
	ora.b $A1
	sta.b $A1
	; Start => Select
	txa
	and.w #$1000
	asl a
	ora.b $A1
	sta.b $A1
	; L => R
	txa
	and.w #$20
	lsr a
	ora.b $A1
	sta.b $A1
	; R => L
	txa
	and.w #$10
	asl a
	ora.b $A1
	sta.b $A1
	
	; UP => DOWN
	txa
	and.w #$800
	lsr a
	ora.b $A1
	sta.b $A1
	; DOWN => UP
	txa
	and.w #$400
	asl a
	ora.b $A1
	sta.b $A1
	;MOON WALK CHECK
	txa
	ldy.w moon
	cpy.w #$2
	beq .C
	;LEFT => Right
	and.w #$200
	lsr a
	ora.b $A1
	sta.b $A1
	;Right => LEFT
	txa
	and.w #$100
	asl a
	ora.b $A1
	sta.b $A1
	ldx $A1
	txa
	rts
.C
	and.w #$300
	ora.b $A1
	sta.b $A1
	ldx $A1
	txa
	rts
	warnpc $C07FFF
	

	org $C0FE4E

	org $C10CE7
	lda.w jumpH
	
	org $C10F2F
	jml WeaponLockSpawn
	nop
ReturnBack:
	
	org $C110C3
	jmp WeaponSwap2
	
	org $C12BF7
	jsr WeaponSwap1
	
	org $C12C14
	jmp WeaponLockCheck
	
	org $C12FF0
	jsr WeaponEquiped
	
	org $C13069
	jmp MoonWalkCheck
	nop
	
	org $C132D2
	jmp SlideDisableCheck
	nop

	org $C14AAA
	jsr WeaponEquiped2

	org $C17F60
WeaponSwap1:	;For regular MegaMan
	lda.w NewWeaponF
	beq .S
	jsr AssignMegaNewWeapon
	lda.b #$14
	jsl $C03205
.S
	lda.w $BFB
	rts
WeaponSwap2:	;For Super Adapter
	lda.w NewWeaponF
	beq .S
	jsr AssignMegaNewWeapon
	lda.b #$14
	jsl $C03205
.S
	jmp $3728

AssignMegaNewWeapon:
	stz.w NewWeaponF
	ldx.w NewWeapon
	cpx.w $BC7
	bne .G
	rts
.G	
	stz.w $C5F
	stz.w $C70
	jsl SetupMegaWeaponPAL
	ldx.w NewWeapon
	lda.w $9FCE,X
	sta.w $BCF
	lda.w $9FDD,X
	sta.w $BD6
	ldx.w $BC7
	phx
	jml MegaManVarAssign
Leave:
	plx
	cpx.b #$E
	bne .S3
	jsr $0D2B
.S3
	lda.b #$FF
	sta.w $BD2 ;Prev Ammo
	rts
WeaponLockCheck:
	lda.w WeaponL
	beq .S
	rts
.S
	lda $BC7
	jmp $2C17
SlideDisableCheck:
	lda.w slideF
	beq .S
	jmp $32DC ;SKIP
.S
	stz $62	;$C00+$62
	lda.b #$C
	sta $2
	stz $3
	stz $29
	jmp $32DC
MoonWalkCheck:
	bit.w moon
	bne .S
	lda $2	;MegaMan State

	jmp $306D
.S	
	jmp $3074
WeaponEquiped2:
	pha
	lda.w $BC7
	asl : tay
	pla
WeaponEquiped:
	ldx.w $B83,Y
	bpl .L
	ora.w #$8000
.L
	rts
	warnpc $C17FFF


	org $C254B9
	jsr OHKO_Check	


	org $C2FF5C
OHKO_Check:
	lda.w OHKO_F
	beq .S
	lda.b #$1C
	rts
.S
	lda.w $C2E	;MegaMan HP
	rts
	
	
	org $C304AC
	jsl ObjectImmuneCheck

	org $C3FF9D
	warnpc $C3FFFF


	org $D8EF15
SetupMegaWeaponPAL:
	lda.w $9FEC,X
	sta.w $C5E
	and.b #$FF
	ldx.b #$10
	jsl $C008F4 ;Load Pallete A&X
	ldx.w NewWeapon
	lda.w $9FFB,X
	and.b #$FF
	ldx.b #$20
	jsl $C008F4
	ldx.w NewWeapon
	lda.w $9FCE,X
	sta.w $BCF
	lda.w $9FDD,X
	sta.w $BD6
	rtl
CNT_INV_Preserve:	;Backup
	jsl $C03205 ;Play Sound
	lda.w CNT_INV
	sta.w $5E0
	lda.w CNT_SWP
	sta.w $5E1
	stz.w CNT_INV
	stz.w CNT_SWP
	rtl
CNT_INV_Restorte: ;Restore
	lda.w $5E0
	sta.w CNT_INV
	lda.w $5E1
	sta.w CNT_SWP
	lda.l $7FB3D0
	rtl
	warnpc $D8FFFF

