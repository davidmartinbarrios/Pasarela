Option Strict Off
Option Explicit On
Imports VB = Microsoft.VisualBasic
Friend Class clsValidaciones
	'Validaciones de Procedimientos
	Private Const cstrDescValPR01 As String = "PR01 - Existe m�s de un objeto procedimiento en siguiente diagrama: "
	Private Const cstrDescValPR02 As String = "PR02 - El siguiente diagrama no tiene Objeto Padre: "
	Private Const cstrDescValPR03 As String = "PR03 - El siguiente procedimiento no tiene c�digo de procedimiento asignado: "
	Private Const cstrDescValPR04 As String = "PR04 - El siguiente procedimiento no es �nico en el conjunto de diagramas: "
	Private Const cstrDescValPR05 As String = "PR05 - El Nivel de autorizaci�n del siguiente procedimiento deber�a contener un valor diferente a 0: "
	Private Const cstrDescValPR06 As String = "PR06 - El siguiente procedimiento no pertenece a ning�n grupo: "
	Private Const cstrDescValPR07 As String = "PR07 - El siguiente procedimiento no est� dado de alta en parametrizaci�n: "
	
	'Validaciones de Tr�mites
	Private Const cstrDescValTR01 As String = "TR01 - El siguiente tr�mite no tiene Nombre: "
	Private Const cstrDescValTR02 As String = "TR02 - El siguiente tr�mite no tiene Categor�a: "
	Private Const cstrDescValTR03 As String = "TR03 - El siguiente tr�mite no existe en la versi�n actual del procedimiento. Pasarela lo dar� de alta: "
	Private Const cstrDescValTR04 As String = "TR04 - El siguiente tr�mite tiene vac�a la propiedad Nombre Tr�mite: "
	
	'Nombres de campos
	Private Const cstrCampoTramite_CAT_DIAGRAMA As String = "CAT_DIAGRAMA"
	Private Const cstrCampoTramite_NOMBRE As String = "NOMBRE"
	Private Const cstrCampoTramite_NIVEL As String = "NIVEL"
	Private Const cstrCampoTramite_ARBOL As String = "ARBOL"
	Private Const cstrCampoTramite_USERDEFINED As String = "USERDEFINED"
	Private Const cstrCampoCM_ID_OBJETO As String = "ANO_ID"
	Private Const cstrCampoCM_USERDEFINED As String = "USERDEFINED"
	Private Const cstrCampoCM_NombreObjeto As String = "GO_NAME"
	
	'Objeto Clase Carga Datos
	Private objCargaDatos As New clsCargaDatos
	
	'Etiquetas de propiedades de procedimiento
	Private Const cstrEtiquetaPropNivAut As String = "NIVELAUTORIZACI�NINICIO"
	Private Const cstrEtiquetaPropGrupo As String = "Grupo Procedimiento"
	Private Const cstrEtiquetaPropCodProce As String = "C�digo Procedimiento"
	Private Const cstrEtiquetaTramNombreTramite As String = "ANOMBRETR�MITE"
	
	'Tipos de datos de propiedades
	Private Const cstrTipoDatoPropNumero As Short = 3
	Private Const cstrTipoDatoPropTexto As Short = 3
	Private Const cstrTipoDatoPropCheck As Short = 5
	Private Const cstrTipoDatoPropLista As Short = 9
	
	'Tipos de objetos
	Private Const cstrTipoObjProcedimiento As Short = 20003
	Private Const cintTipoObjProceso As Short = 8953
	
	'--------------------------------------------------------------------------------------
	'<comment>
	'<summary>Funci�n que lanza la carga de tablas y las validaciones</summary>
	'<param name="colEntrada">Colecci�n de diagramas</param>
	'<return> Boolean:
	'&lt;li&gt; True: Se ha incumplido alguna de las validaciones
	'&lt;li&gt; False: No se ha incumplido ninguna validaci�n
	'</return>
	'<remarks>
	'&lt;li&gt; 1.0     J. Prieto   07/06/2011  Creaci�n
	'</remarks>
	'</comment>
	'--------------------------------------------------------------------------------------
	Public Function fValidacionesIniciales(ByVal colEntrada As Collection) As Boolean
		Dim I As Short
		
		On Error GoTo ERROR_Renamed
		
		'Realizo la carga de las tablas necesarias para las validaciones
		For I = 1 To colEntrada.Count()
			
			'UPGRADE_WARNING: Couldn't resolve default property of object colEntrada()(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			objCargaDatos.fCargaDatosCorporate(colEntrada.Item(I)(0))
			'Validaciones por procedimiento
			'UPGRADE_WARNING: Couldn't resolve default property of object colEntrada()(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If fValidaProcedimiento(CShort(colEntrada.Item(I)(0))) Then
				fValidacionesIniciales = True
			End If
			'Validaciones por tr�mite
			If fValidaTramites Then
				fValidacionesIniciales = True
			End If
			'Validaciones por conector
			If fValidaConectores Then
				fValidacionesIniciales = True
			End If
			'Validaciones por acci�n
			If fValidaAcciones Then
				fValidacionesIniciales = True
			End If
			'Validaciones por UT
			If fValidaUTs Then
				fValidacionesIniciales = True
			End If
		Next 
		
		objCargaDatos.fDescargaDatosCorporate()
		'UPGRADE_NOTE: Object objCargaDatos may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		objCargaDatos = Nothing
		Exit Function
		
ERROR_Renamed: 
		fValidacionesIniciales = False
		'UPGRADE_NOTE: Object objCargaDatos may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		objCargaDatos = Nothing
		If Err.Source <> "" Then
			Err.Raise(Err.Number, Err.Source, Err.Description)
		Else
			Err.Raise(Err.Number, "fValidacionesIniciales", Err.Description)
		End If
		
	End Function
	
	'--------------------------------------------------------------------------------------
	'<comment>
	'<summary>Funci�n que lanza las validaciones por Procedimiento</summary>
	'<param name="intDiagrama">ID del diagrama a validar</param>
	'<return> Boolean:
	'&lt;li&gt; True: Se ha incumplido alguna de las validaciones
	'&lt;li&gt; False: No se ha incumplido ninguna validaci�n
	'</return>
	'<remarks>
	'&lt;li&gt; 1.0     J. Prieto   07/06/2011  Creaci�n
	'</remarks>
	'</comment>
	'--------------------------------------------------------------------------------------
	Private Function fValidaProcedimiento(ByRef intDiagrama As Short) As Boolean
		Dim strSQL As String
		Dim rstProced As ADODB.Recordset
		Dim strValidacion As String
		Dim intIDObjeto As Short 'ANO_ID del tipo objeto procedimiento
		Dim blnInsertaValidacion As Boolean
		Dim strUserdefined As String
		Dim strGrupo As String
		Dim strNivAut As String
		Dim strCodProce As String
		Dim strNomProce As String
		Dim strScriptName As String
		
		On Error GoTo ERROR_Renamed
		
		'*********************************************************************************************************************
		'PR01: un objeto procedimiento por diagrama
		strSQL = "SELECT * FROM DIAGRAM WHERE MODEL_NAME='" & strModelo & "' AND DI_ID = " & intDiagrama
		rstProced = mfRecordset(conDP4, strSQL)
		If Not rstProced.EOF Then
			If rstProced.RecordCount > 1 Then
				strValidacion = cstrDescValPR01 & intDiagrama
				blnInsertaValidacion = True
				fValidaProcedimiento = True
			End If
			intIDObjeto = rstProced.Fields(cstrCampoCM_ID_OBJETO).Value
		End If
		CierraRS(rstProced)
		If blnInsertaValidacion = True Then
			sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaProcedimiento")
		End If
		blnInsertaValidacion = False
		
		'*********************************************************************************************************************
		'PR02: el diagrama no tiene Objeto Padre
		If intIDObjeto = 0 Then
			strValidacion = cstrDescValPR02 & intDiagrama
			blnInsertaValidacion = True
			fValidaProcedimiento = True
			If blnInsertaValidacion = True Then
				sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaProcedimiento")
			End If
			'Es una validaci�n cr�tica dentro de las validaciones de procedimiento, debe acabar
			'UPGRADE_NOTE: Object rstProced may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
			rstProced = Nothing
			Exit Function
		End If
		
		'*********************************************************************************************************************
		'PR03: debe tener c�digo procedimiento en el XML
		strSQL = "SELECT * FROM CW_OBJECT WHERE MODEL_NAME='" & strModelo & "' AND GO_ID=" & intIDObjeto & " AND OT_ID=" & cstrTipoObjProcedimiento
		rstProced = mfRecordset(conDP4, strSQL)
		If Not rstProced.EOF Then
			strUserdefined = rstProced.Fields(cstrCampoCM_USERDEFINED).Value
			strNomProce = rstProced.Fields(cstrCampoCM_NombreObjeto).Value
		End If
		CierraRS(rstProced)
		
		strSQL = "SELECT PPT_SCRPT_NAME FROM dbo.CW_PROP_TYPE " & "WHERE PPT_NAME='" & cstrEtiquetaPropCodProce & "' " & "AND MODEL_NAME='" & strModelo & "' " & "AND OT_ID=" & cstrTipoObjProcedimiento
		rstProced = mfRecordset(conDP4, strSQL)
		If Not rstProced.EOF Then
			strScriptName = rstProced.Fields("PPT_SCRPT_NAME").Value
		End If
		
		strCodProce = objCargaDatos.fBuscaDatoXML(strUserdefined, strScriptName, cstrTipoDatoPropTexto)
		If strCodProce = "" Then
			strValidacion = cstrDescValPR03 & strNomProce
			blnInsertaValidacion = True
			fValidaProcedimiento = True
		Else
			gNomProcCorto = strCodProce
		End If
		CierraRS(rstProced)
		If blnInsertaValidacion = True Then
			sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaProcedimiento")
		End If
		blnInsertaValidacion = False
		
		'*********************************************************************************************************************
		'PR04: El procedimiento debe ser �nico en el conjunto de diagramas
		strSQL = "SELECT * FROM SHAPE S,DIAGRAM D" & " WHERE S.MODEL_NAME='ARDATZ' AND D.MODEL_NAME='" & strModelo & "'" & " AND S.ANO_ID=D.ANO_ID" & " AND S.DI_ID=D.DI_ID" & " AND S.ANO_TABNR=" & cstrTipoObjProcedimiento & " AND S.ANO_ID=" & intIDObjeto
		rstProced = mfRecordset(conDP4, strSQL)
		
		If Not rstProced.EOF Then
			If rstProced.RecordCount > 1 Then
				strValidacion = cstrDescValPR04 & strNomProce
				blnInsertaValidacion = True
				fValidaProcedimiento = True
			End If
		End If
		CierraRS(rstProced)
		If blnInsertaValidacion = True Then
			sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaProcedimiento")
		End If
		blnInsertaValidacion = False
		
		'*********************************************************************************************************************
		'PR05: Nivel de autorizaci�n <>0
		'    If strUserdefined = "" Then
		'        strSQL = "SELECT * FROM CW_OBJECT WHERE MODEL_NAME='" & strModelo & "' AND GO_ID=" & intIDObjeto & " AND OT_ID=" & cstrTipoObjProcedimiento
		'        Set rstProced = mfRecordset(conDP4, strSQL)
		'        If Not rstProced.EOF Then
		'            strUserdefined = rstProced(cstrCampoCM_USERDEFINED)
		'        End If
		'    End If
		'
		'    strNivAut = objCargaDatos.fBuscaDatoXML(strUserdefined, cstrEtiquetaPropNivAut, cstrTipoDatoPropNumero)
		'    If Val(strNivAut) = 0 Then
		'        strValidacion = cstrDescValPR05 & strNomProce
		'        blnInsertaValidacion = True
		'        fValidaProcedimiento = True
		'    End If
		'    CierraRS rstProced
		'    If blnInsertaValidacion = True Then
		'        sInsertaValidacion gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaProcedimiento"
		'    End If
		'    blnInsertaValidacion = False
		
		'*********************************************************************************************************************
		'PR06: Que pertenezca a un grupo
		If strUserdefined = "" Then
			strSQL = "SELECT * FROM CW_OBJECT WHERE MODEL_NAME='" & strModelo & "' AND GO_ID=" & intIDObjeto & " AND OT_ID=" & cstrTipoObjProcedimiento
			rstProced = mfRecordset(conDP4, strSQL)
			If Not rstProced.EOF Then
				strUserdefined = rstProced.Fields(cstrCampoCM_USERDEFINED).Value
			End If
		End If
		CierraRS(rstProced)
		
		strSQL = "SELECT PPT_SCRPT_NAME FROM dbo.CW_PROP_TYPE " & "WHERE PPT_NAME='" & cstrEtiquetaPropGrupo & "' " & "AND MODEL_NAME='" & strModelo & "' " & "AND OT_ID=" & cstrTipoObjProcedimiento
		rstProced = mfRecordset(conDP4, strSQL)
		If Not rstProced.EOF Then
			strScriptName = rstProced.Fields("PPT_SCRPT_NAME").Value
		End If
		
		strGrupo = objCargaDatos.fBuscaDatoXML(strUserdefined, strScriptName, cstrTipoDatoPropNumero)
		If Trim(strGrupo) = "" Then
			strValidacion = cstrDescValPR06 & strNomProce
			blnInsertaValidacion = True
			fValidaProcedimiento = True
		End If
		CierraRS(rstProced)
		If blnInsertaValidacion = True Then
			sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaProcedimiento")
		End If
		blnInsertaValidacion = False
		
		'*********************************************************************************************************************
		'PR07: que est� dado de alta en parametrizaci�n (TBPFIN01)
		strSQL = "SELECT * FROM %owner%TBPFIN01 WHERE CODEXTPR='" & gNomProcCorto & "'" & " AND CODENTID=" & strCodEntid & " AND CODFAMIL=" & strCodFamil & ""
		rstProced = mfRecordset(conDB2, strSQL, "DB2")
		If rstProced.EOF Then
			strValidacion = cstrDescValPR07 & strNomProce
			blnInsertaValidacion = True
			fValidaProcedimiento = True
		End If
		CierraRS(rstProced)
		If blnInsertaValidacion = True Then
			sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaProcedimiento")
		End If
		'*********************************************************************************************************************
		'UPGRADE_NOTE: Object rstProced may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		rstProced = Nothing
		Exit Function
		
ERROR_Renamed: 
		'UPGRADE_NOTE: Object rstProced may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		rstProced = Nothing
		fValidaProcedimiento = False
		If Err.Source <> "" Then
			Err.Raise(Err.Number, Err.Source, Err.Description)
		Else
			Err.Raise(Err.Number, "fValidaProcedimiento", Err.Description)
		End If
		
	End Function
	
	'--------------------------------------------------------------------------------------
	'<comment>
	'<summary>Funci�n que lanza las validaciones por Tr�mite</summary>
	'<return> Boolean:
	'&lt;li&gt; True: Se ha incumplido alguna de las validaciones
	'&lt;li&gt; False: No se ha incumplido ninguna validaci�n
	'</return>
	'<remarks>
	'&lt;li&gt; 1.0     J. Prieto   07/06/2011  Creaci�n
	'</remarks>
	'</comment>
	'--------------------------------------------------------------------------------------
	Private Function fValidaTramites() As Boolean
		Dim strUserdefined As Object
		Dim strSQL As String
		Dim rstTramites As ADODB.Recordset
		Dim rstTBPFIN03 As ADODB.Recordset
		Dim strValidacion As String
		Dim arrArbolTra() As String
		Dim strNombreTramite As String
		
		On Error GoTo ERROR_Renamed
		
		strSQL = "SELECT * FROM DIAGRAMA WHERE PROCEDIMIENTO='" & gNomProcCorto & "'"
		rstTramites = mfRecordset(conPasarela, strSQL)
		If Not rstTramites.EOF Then
			
			While Not rstTramites.EOF
				'La �ltima posici�n del array ser� el ID del tr�mite
				arrArbolTra = Split(rstTramites.Fields(cstrCampoTramite_ARBOL).Value, "-")
				'*********************************************************************************************************************
				'TR01: debe tener Nombre
				If Trim(rstTramites.Fields(cstrCampoTramite_NOMBRE).Value) = "" Then
					'No tiene Nombre, inserto validaci�n
					strValidacion = cstrDescValTR01 & "Nivel:" & rstTramites.Fields(cstrCampoTramite_NIVEL).Value & ", ID:" & arrArbolTra(UBound(arrArbolTra))
					fValidaTramites = True
					sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaTramites")
				End If
				'*********************************************************************************************************************
				'TR02: debe tener Categor�a
				If Trim(rstTramites.Fields(cstrCampoTramite_CAT_DIAGRAMA).Value) = "" Then
					'No tiene Categor�a, inserto validaci�n
					strValidacion = cstrDescValTR02 & "Nivel:" & rstTramites.Fields(cstrCampoTramite_NIVEL).Value & ", ID:" & arrArbolTra(UBound(arrArbolTra)) & ", Nombre:" & rstTramites.Fields(cstrCampoTramite_NOMBRE).Value
					fValidaTramites = True
					sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaTramites")
				End If
				'*********************************************************************************************************************
				'TR03: Alta en parametrizaci�n (TBPFIN03)
				'Si su categr�a es Ramificaci�n no debe hacer esta validaci�n
				If Trim(rstTramites.Fields(cstrCampoTramite_CAT_DIAGRAMA).Value) <> "Ramificaci�n" Then
					'Consulta a las tablas de parametrizaci�n, si no devuelve datos es que el tr�mite no est� parametrizado
					strSQL = "SELECT * FROM %owner%TBPFIN03" & " WHERE CODPROCE=(SELECT CODPROCE FROM %owner%TBPFIN01 WHERE CODEXTPR='" & gNomProcCorto & "'" & " AND CODENTID=" & strCodEntid & " AND CODFAMIL=" & strCodFamil & ")" & " AND VERSIONP=(SELECT MAX(VERSIONP) FROM %owner%TBPFIN03 WHERE" & " CODPROCE=(SELECT CODPROCE FROM %owner%TBPFIN01 WHERE CODEXTPR='" & gNomProcCorto & "'" & " AND CODENTID=" & strCodEntid & " AND CODFAMIL=" & strCodFamil & "))" & " AND CDESTRAM=" & arrArbolTra(UBound(arrArbolTra))
					rstTBPFIN03 = mfRecordset(conDB2, strSQL, "DB2")
					If rstTBPFIN03.EOF Then
						'No est� parametrizado, inserto validaci�n
						strValidacion = cstrDescValTR03 & "Nivel:" & rstTramites.Fields(cstrCampoTramite_NIVEL).Value & ", ID:" & arrArbolTra(UBound(arrArbolTra)) & ", Nombre:" & rstTramites.Fields(cstrCampoTramite_NOMBRE).Value
						fValidaTramites = True
						sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaTramites")
					End If
				End If
				'*********************************************************************************************************************
				'TR04: Debe tener Nombre Tr�mite (Propiedad ANOMBRETR�MITE de USERDEFINED)
				'Si su categr�a es Ramificaci�n no debe hacer esta validaci�n
				If Trim(rstTramites.Fields(cstrCampoTramite_CAT_DIAGRAMA).Value) <> "Ramificaci�n" Then
					'UPGRADE_WARNING: Couldn't resolve default property of object strUserdefined. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					strUserdefined = rstTramites.Fields(cstrCampoTramite_USERDEFINED).Value
					'UPGRADE_WARNING: Couldn't resolve default property of object strUserdefined. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					If strUserdefined <> "" Then
						'UPGRADE_WARNING: Couldn't resolve default property of object strUserdefined. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
						strNombreTramite = objCargaDatos.fBuscaDatoXML(strUserdefined, cstrEtiquetaTramNombreTramite, cstrTipoDatoPropLista)
						If strNombreTramite = "" Then
							'No tiene valor en la propiedad Nombre Tr�mite
							strValidacion = cstrDescValTR04 & "Nivel:" & rstTramites.Fields(cstrCampoTramite_NIVEL).Value & ", ID:" & arrArbolTra(UBound(arrArbolTra)) & ", Nombre:" & rstTramites.Fields(cstrCampoTramite_NOMBRE).Value
							fValidaTramites = True
							sInsertaValidacion(gNomProcCorto, strUserId, "Validaciones", strValidacion, "fValidaTramites")
						End If
					End If
				End If
				'*********************************************************************************************************************
				rstTramites.MoveNext()
			End While
		End If
		CierraRS(rstTramites)
		CierraRS(rstTBPFIN03)
		
		'UPGRADE_NOTE: Object rstTramites may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		rstTramites = Nothing
		'UPGRADE_NOTE: Object rstTBPFIN03 may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		rstTBPFIN03 = Nothing
		Exit Function
		
ERROR_Renamed: 
		'UPGRADE_NOTE: Object rstTramites may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		rstTramites = Nothing
		'UPGRADE_NOTE: Object rstTBPFIN03 may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		rstTBPFIN03 = Nothing
		fValidaTramites = False
		If Err.Source <> "" Then
			Err.Raise(Err.Number, Err.Source, Err.Description)
		Else
			Err.Raise(Err.Number, "fValidaTramites", Err.Description)
		End If
		
	End Function
	
	'--------------------------------------------------------------------------------------
	'<comment>
	'<summary>Funci�n que lanza las validaciones por Procedimiento</summary>
	'<return> Boolean:
	'&lt;li&gt; True: Se ha incumplido alguna de las validaciones
	'&lt;li&gt; False: No se ha incumplido ninguna validaci�n
	'</return>
	'<remarks>
	'&lt;li&gt; 1.0     J. Prieto   07/06/2011  Creaci�n
	'</remarks>
	'</comment>
	'--------------------------------------------------------------------------------------
	Public Function fValidaConectores() As Boolean
		
		On Error GoTo ERROR_Renamed
		
		fValidaConectores = False
		Exit Function
		
ERROR_Renamed: 
		fValidaConectores = False
		If Err.Source <> "" Then
			Err.Raise(Err.Number, Err.Source, Err.Description)
		Else
			Err.Raise(Err.Number, "sInsertaValidacion", Err.Description)
		End If
		
	End Function
	
	'--------------------------------------------------------------------------------------
	'<comment>
	'<summary>Funci�n que lanza las validaciones por Procedimiento</summary>
	'<return> Boolean:
	'&lt;li&gt; True: Se ha incumplido alguna de las validaciones
	'&lt;li&gt; False: No se ha incumplido ninguna validaci�n
	'</return>
	'<remarks>
	'&lt;li&gt; 1.0     J. Prieto   07/06/2011  Creaci�n
	'</remarks>
	'</comment>
	'--------------------------------------------------------------------------------------
	Public Function fValidaAcciones() As Boolean
		
		On Error GoTo ERROR_Renamed
		
		fValidaAcciones = False
		Exit Function
		
ERROR_Renamed: 
		fValidaAcciones = False
		If Err.Source <> "" Then
			Err.Raise(Err.Number, Err.Source, Err.Description)
		Else
			Err.Raise(Err.Number, "sInsertaValidacion", Err.Description)
		End If
		
	End Function
	
	'--------------------------------------------------------------------------------------
	'<comment>
	'<summary>Funci�n que lanza las validaciones por Procedimiento</summary>
	'<param name="intDiagrama">ID del diagrama a validar</param>
	'<return> Boolean:
	'&lt;li&gt; True: Se ha incumplido alguna de las validaciones
	'&lt;li&gt; False: No se ha incumplido ninguna validaci�n
	'</return>
	'<remarks>
	'&lt;li&gt; 1.0     J. Prieto   07/06/2011  Creaci�n
	'</remarks>
	'</comment>
	'--------------------------------------------------------------------------------------
	Public Function fValidaUTs() As Boolean
		
		On Error GoTo ERROR_Renamed
		
		fValidaUTs = False
		Exit Function
		
ERROR_Renamed: 
		fValidaUTs = False
		If Err.Source <> "" Then
			Err.Raise(Err.Number, Err.Source, Err.Description)
		Else
			Err.Raise(Err.Number, "sInsertaValidacion", Err.Description)
		End If
		
	End Function
	
	'--------------------------------------------------------------------------------------
	'<comment>
	'<summary>Funci�n que lanza las validaciones por Procedimiento</summary>
	'<param name="pstrProcedimiento">Procedimiento al que pertenece</param>
	'<param name="pstrUserId">Usuario que lo ha ejecutado</param>
	'<param name="pstrAmbito">�mbito al que pertenece</param>
	'<param name="pstrMensaje">Contenido del mensaje</param>
	'<param name="pstrLugar">Funci�n en la que se ha producido</param>
	'<remarks>
	'&lt;li&gt; 1.0     J. Prieto   07/06/2011  Creaci�n
	'</remarks>
	'</comment>
	'--------------------------------------------------------------------------------------
	Private Sub sInsertaValidacion(ByRef pstrProcedimiento As String, ByRef pstrUserId As String, ByRef pstrAmbito As String, ByRef pstrMensaje As String, ByRef pstrLugar As String)
		
		Dim strSQL As String
		Dim strFecha As String
		
		On Error GoTo ERROR_Renamed
		
		strFecha = VB6.Format(Year(Now), "0000") & "-" & VB6.Format(Month(Now), "00") & "-" & VB6.Format(VB.Day(Now), "00")
		
		strSQL = "INSERT INTO ERRORES (PROCEDIMIENTO,USERID,AMBITO,DESCRIPCION,FECHA,LUGAR)" & " VALUES ('" & pstrProcedimiento & "','" & pstrUserId & "','" & pstrAmbito & "','" & pstrMensaje & "','" & strFecha & "','" & pstrLugar & "')"
		mfExecute(conPasarela, strSQL)
		Exit Sub
		
ERROR_Renamed: 
		If Err.Source <> "" Then
			Err.Raise(Err.Number, Err.Source, Err.Description)
		Else
			Err.Raise(Err.Number, "sInsertaValidacion", Err.Description)
		End If
		
	End Sub
End Class