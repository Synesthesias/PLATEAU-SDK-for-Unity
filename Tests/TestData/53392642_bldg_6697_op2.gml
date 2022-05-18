<?xml version="1.0" encoding="UTF-8"?>
<core:CityModel xmlns:grp="http://www.opengis.net/citygml/cityobjectgroup/2.0" xmlns:core="http://www.opengis.net/citygml/2.0" xmlns:pbase="http://www.opengis.net/citygml/profiles/base/2.0" xmlns:smil20lang="http://www.w3.org/2001/SMIL20/Language" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:smil20="http://www.w3.org/2001/SMIL20/" xmlns:bldg="http://www.opengis.net/citygml/building/2.0" xmlns:xAL="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0" xmlns:uro="http://www.kantei.go.jp/jp/singi/tiiki/toshisaisei/itoshisaisei/iur/uro/1.4" xmlns:luse="http://www.opengis.net/citygml/landuse/2.0" xmlns:app="http://www.opengis.net/citygml/appearance/2.0" xmlns:gen="http://www.opengis.net/citygml/generics/2.0" xmlns:dem="http://www.opengis.net/citygml/relief/2.0" xmlns:tex="http://www.opengis.net/citygml/texturedsurface/2.0" xmlns:tun="http://www.opengis.net/citygml/tunnel/2.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:sch="http://www.ascc.net/xml/schematron" xmlns:veg="http://www.opengis.net/citygml/vegetation/2.0" xmlns:frn="http://www.opengis.net/citygml/cityfurniture/2.0" xmlns:gml="http://www.opengis.net/gml" xmlns:tran="http://www.opengis.net/citygml/transportation/2.0" xmlns:wtr="http://www.opengis.net/citygml/waterbody/2.0" xmlns:brid="http://www.opengis.net/citygml/bridge/2.0" xsi:schemaLocation="http://www.kantei.go.jp/jp/singi/tiiki/toshisaisei/itoshisaisei/iur/uro/1.4 http://www.kantei.go.jp/jp/singi/tiiki/toshisaisei/itoshisaisei/iur/schemas/uro/1.4/urbanObject.xsd http://www.opengis.net/citygml/2.0 http://schemas.opengis.net/citygml/2.0/cityGMLBase.xsd http://www.opengis.net/citygml/landuse/2.0 http://schemas.opengis.net/citygml/landuse/2.0/landUse.xsd http://www.opengis.net/citygml/building/2.0 http://schemas.opengis.net/citygml/building/2.0/building.xsd http://www.opengis.net/citygml/transportation/2.0 http://schemas.opengis.net/citygml/transportation/2.0/transportation.xsd http://www.opengis.net/citygml/generics/2.0 http://schemas.opengis.net/citygml/generics/2.0/generics.xsd http://www.opengis.net/citygml/cityobjectgroup/2.0 http://schemas.opengis.net/citygml/cityobjectgroup/2.0/cityObjectGroup.xsd http://www.opengis.net/gml http://schemas.opengis.net/gml/3.1.1/base/gml.xsd http://www.opengis.net/citygml/appearance/2.0 http://schemas.opengis.net/citygml/appearance/2.0/appearance.xsd">
	<gml:boundedBy>
		<gml:Envelope srsName="http://www.opengis.net/def/crs/EPSG/0/6697" srsDimension="3">
			<gml:lowerCorner>35.538213152192796 139.77492484044808 2.466</gml:lowerCorner>
			<gml:upperCorner>35.54192121842839 139.77926068928994 19.66144493</gml:upperCorner>
		</gml:Envelope>
	</gml:boundedBy>
	<core:cityObjectMember>
		<bldg:Building gml:id="BLD_0772bfd9-fa36-4747-ad0f-1e57f883f745">
			<gen:doubleAttribute name="doubleAttributeテスト">
				<gen:value>123.456</gen:value>
			</gen:doubleAttribute>
			<gen:intAttribute name="intAttributeテスト">
				<gen:value>123</gen:value>
			</gen:intAttribute>
			<gen:stringAttribute name="建物ID">
				<gen:value>13111-bldg-147301</gen:value>
			</gen:stringAttribute>
			<gen:genericAttributeSet name="多摩水系多摩川、浅川、大栗川洪水浸水想定区域（想定最大規模）">
				<gen:stringAttribute name="規模">
					<gen:value>L2</gen:value>
				</gen:stringAttribute>
				<gen:stringAttribute name="浸水ランク">
					<gen:value>1</gen:value>
				</gen:stringAttribute>
				<gen:measureAttribute name="浸水深">
					<gen:value uom="m">0.050</gen:value>
				</gen:measureAttribute>
			</gen:genericAttributeSet>
			<bldg:measuredHeight uom="m">2.8（テスト）</bldg:measuredHeight>
			<bldg:lod0RoofEdge>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>35.538511979563104 139.77765001365444 5.9350000000000005 35.538564738145176 139.77761107976545 5.9350000000000005 35.53860674525613 139.77758071994626 5.9350000000000005 35.538607003314475 139.7775812822042 5.9350000000000005 35.53861053745228 139.77757884694498 5.9350000000000005 35.53859090520818 139.7775361782256 5.9350000000000005 35.538587371071735 139.7775386145889 5.9350000000000005 35.5385884908272 139.7775410499349 5.9350000000000005 35.53857689467651 139.777549430546 5.9350000000000005 35.53857665737096 139.77754891348948 5.9350000000000005 35.538530785801086 139.77758275206293 5.9350000000000005 35.538521478453596 139.77758954853826 5.9350000000000005 35.53849370611842 139.77761010768472 5.9350000000000005 35.538511979563104 139.77765001365444 5.9350000000000005</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
							<gml:interior>
								<gml:LinearRing gml:id="ID_test_interior_ring_1">
									<gml:posList>35.45467922889754 139.63076484097758 28.238600000000005 35.454671758804935 139.63075106225995 28.2386 35.45466428871075 139.63073728354485 28.238600000000005 35.45463479893571 139.6306828892091 28.238600000000005 35.45462695081925 139.630668413254 28.2386 35.454619102701045 139.63065393730173 28.238600000000005 35.45459416714607 139.63060794339833 28.2386 35.45458699721838 139.63059471839964 28.2386 35.454589980458366 139.6305923025542 28.2386 35.454739519164754 139.63047120494159 28.2386 35.454740148539905 139.6304706952675 28.2386 35.45474126880615 139.63047276160702 28.2386 35.454772254082805 139.63052991423191 28.238599999999998 35.45478010221573 139.63054439019945 28.2386 35.45478795034691 139.6305588661698 28.2386 35.454817440177294 139.6306132605628 28.2386 35.45482491028548 139.6306270392924 28.2386 35.45483238039211 139.63064081802455 28.2386 35.454867343048505 139.63070530728072 28.238600000000005 35.45471419148835 139.63082933016585 28.238600000000005 35.45467922889754 139.63076484097758 28.238600000000005</gml:posList>
								</gml:LinearRing>
							</gml:interior>
							<gml:interior>
								<gml:LinearRing gml:id="ID_test_interior_ring_2">
									<gml:posList>35.4550195558222 139.6313958081286 28.238600000000005 35.455005770051464 139.63137031838562 28.238600000000005 35.45499666880805 139.63135349029392 28.238600000000005 35.45499411863578 139.63134877505695 28.238600000000005 35.45498496686599 139.63133185355306 28.2386 35.454978924151504 139.63132068032954 28.238600000000005 35.45494478380748 139.6313482603742 28.2386 35.45491483128418 139.63129287774686 28.238600000000005 35.454906196981725 139.63127691281935 28.2386 35.45490098990281 139.63126728487225 28.238600000000005 35.45486904567209 139.6312930907051 28.238600000000005 35.454830266967285 139.6312229010874 28.238600000000005 35.45486530830963 139.6311945932513 28.238600000000005 35.454862745929816 139.63118985539322 28.2386 35.45485471124599 139.63117499921114 28.2386 35.45482266974897 139.63111575434002 28.2386 35.45485428042701 139.6310902174812 28.238600000000005 35.45484946430417 139.63108131239133 28.238600000000005 35.45484109053793 139.63106582916603 28.238600000000005 35.454831710808904 139.63104848630957 28.238600000000005 35.454823664507835 139.63103360893095 28.238600000000005 35.45481258367467 139.63101312080045 28.238600000000005 35.45480463796296 139.6309984294235 28.238600000000005 35.45479669224943 139.63098373804945 28.238600000000005 35.45476424604766 139.63092374610864 28.238600000000005 35.454917160649266 139.63080021255425 28.2386 35.45494960738281 139.6308602054292 28.2386 35.45495755287572 139.63087489638335 28.2386 35.45496549836684 139.63088958734036 28.2386 35.45499400614163 139.63094229734824 28.2386 35.455002380167834 139.630957780683 28.2386 35.455010754192045 139.63097326402092 28.2386 35.45502769779775 139.63100459229784 28.2386 35.45502867503259 139.63100639917994 28.2386 35.45503923851883 139.63102593080072 28.2386 35.45504727314901 139.6310407866375 28.2386 35.455055307777386 139.63105564247726 28.2386 35.455084617159685 139.63110983489275 28.2386 35.455093252021804 139.63112580058257 28.2386 35.45510135630007 139.63114078523998 28.2386 35.455101886881806 139.63114176627582 28.2386 35.455102446493214 139.631142800987 28.2386 35.45512873009177 139.63119139891143 28.238600000000005 35.45513788188108 139.63120832042932 28.238600000000005 35.455147033668005 139.63122524195106 28.2386 35.45517247090206 139.63127227507232 28.2386 35.45518145002161 139.631288877365 28.2386 35.45519042913887 139.63130547966136 28.238600000000005 35.45522419243991 139.631367907732 28.2386 35.45507127726315 139.6314914406873 28.238600000000005 35.45503751402536 139.63142901268256 28.238600000000005 35.455028534924914 139.63141241040375 28.238600000000005 35.4550195558222 139.6313958081286 28.238600000000005</gml:posList>
								</gml:LinearRing>
							</gml:interior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</bldg:lod0RoofEdge>
			<bldg:lod1Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538511979563104 139.77765001365444 3.189 35.53849370611842 139.77761010768472 3.189 35.538521478453596 139.77758954853826 3.189 35.538530785801086 139.77758275206293 3.189 35.53857665737096 139.77754891348948 3.189 35.53857689467651 139.777549430546 3.189 35.5385884908272 139.7775410499349 3.189 35.538587371071735 139.7775386145889 3.189 35.53859090520818 139.7775361782256 3.189 35.53861053745228 139.77757884694498 3.189 35.538607003314475 139.7775812822042 3.189 35.53860674525613 139.77758071994626 3.189 35.538564738145176 139.77761107976545 3.189 35.538511979563104 139.77765001365444 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538511979563104 139.77765001365444 3.189 35.538564738145176 139.77761107976545 3.189 35.538564738145176 139.77761107976545 5.9350000000000005 35.538511979563104 139.77765001365444 5.9350000000000005 35.538511979563104 139.77765001365444 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538564738145176 139.77761107976545 3.189 35.53860674525613 139.77758071994626 3.189 35.53860674525613 139.77758071994626 5.9350000000000005 35.538564738145176 139.77761107976545 5.9350000000000005 35.538564738145176 139.77761107976545 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53860674525613 139.77758071994626 3.189 35.538607003314475 139.7775812822042 3.189 35.538607003314475 139.7775812822042 5.9350000000000005 35.53860674525613 139.77758071994626 5.9350000000000005 35.53860674525613 139.77758071994626 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538607003314475 139.7775812822042 3.189 35.53861053745228 139.77757884694498 3.189 35.53861053745228 139.77757884694498 5.9350000000000005 35.538607003314475 139.7775812822042 5.9350000000000005 35.538607003314475 139.7775812822042 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53861053745228 139.77757884694498 3.189 35.53859090520818 139.7775361782256 3.189 35.53859090520818 139.7775361782256 5.9350000000000005 35.53861053745228 139.77757884694498 5.9350000000000005 35.53861053745228 139.77757884694498 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53859090520818 139.7775361782256 3.189 35.538587371071735 139.7775386145889 3.189 35.538587371071735 139.7775386145889 5.9350000000000005 35.53859090520818 139.7775361782256 5.9350000000000005 35.53859090520818 139.7775361782256 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538587371071735 139.7775386145889 3.189 35.5385884908272 139.7775410499349 3.189 35.5385884908272 139.7775410499349 5.9350000000000005 35.538587371071735 139.7775386145889 5.9350000000000005 35.538587371071735 139.7775386145889 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.5385884908272 139.7775410499349 3.189 35.53857689467651 139.777549430546 3.189 35.53857689467651 139.777549430546 5.9350000000000005 35.5385884908272 139.7775410499349 5.9350000000000005 35.5385884908272 139.7775410499349 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53857689467651 139.777549430546 3.189 35.53857665737096 139.77754891348948 3.189 35.53857665737096 139.77754891348948 5.9350000000000005 35.53857689467651 139.777549430546 5.9350000000000005 35.53857689467651 139.777549430546 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53857665737096 139.77754891348948 3.189 35.538530785801086 139.77758275206293 3.189 35.538530785801086 139.77758275206293 5.9350000000000005 35.53857665737096 139.77754891348948 5.9350000000000005 35.53857665737096 139.77754891348948 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538530785801086 139.77758275206293 3.189 35.538521478453596 139.77758954853826 3.189 35.538521478453596 139.77758954853826 5.9350000000000005 35.538530785801086 139.77758275206293 5.9350000000000005 35.538530785801086 139.77758275206293 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538521478453596 139.77758954853826 3.189 35.53849370611842 139.77761010768472 3.189 35.53849370611842 139.77761010768472 5.9350000000000005 35.538521478453596 139.77758954853826 5.9350000000000005 35.538521478453596 139.77758954853826 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53849370611842 139.77761010768472 3.189 35.538511979563104 139.77765001365444 3.189 35.538511979563104 139.77765001365444 5.9350000000000005 35.53849370611842 139.77761010768472 5.9350000000000005 35.53849370611842 139.77761010768472 3.189</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538511979563104 139.77765001365444 5.9350000000000005 35.538564738145176 139.77761107976545 5.9350000000000005 35.53860674525613 139.77758071994626 5.9350000000000005 35.538607003314475 139.7775812822042 5.9350000000000005 35.53861053745228 139.77757884694498 5.9350000000000005 35.53859090520818 139.7775361782256 5.9350000000000005 35.538587371071735 139.7775386145889 5.9350000000000005 35.5385884908272 139.7775410499349 5.9350000000000005 35.53857689467651 139.777549430546 5.9350000000000005 35.53857665737096 139.77754891348948 5.9350000000000005 35.538530785801086 139.77758275206293 5.9350000000000005 35.538521478453596 139.77758954853826 5.9350000000000005 35.53849370611842 139.77761010768472 5.9350000000000005 35.538511979563104 139.77765001365444 5.9350000000000005</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod1Solid>
			<bldg:lod2Solid>
				<gml:Solid srsName="http://www.opengis.net/def/crs/EPSG/0/6697">
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_b_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2438_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2437_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2437_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2434_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2438_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2438_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2438_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2438_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2437_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2437_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2437_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2437_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2434_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2434_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2434_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0034_p2434_4"/>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod2Solid>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_9f9d1116-eb56-47fa-bfc4-7c0df1426dd9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_b_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_b_0">
											<gml:posList>35.538610537686885 139.77757884514327 3.29262763 35.53860700340009 139.77758127943486 3.29259311 35.538606745143454 139.77758071818175 3.29260263 35.538564575176174 139.77761119581533 3.29216996 35.53851197919248 139.7776500090309 3.29162384 35.53849370618849 139.77761010541033 3.29230179 35.538521502501546 139.77758960050951 3.29258945 35.538521454419715 139.77758949600295 3.29259123 35.53853078602406 139.777582752198 3.29268598 35.538576657583846 139.77754891342735 3.29316627 35.538576894259855 139.7775494304678 3.29315747 35.53858849171662 139.77754104915252 3.29327693 35.538587370536 139.7775386125623 3.29331844 35.53859090482195 139.77753617826951 3.29335295 35.538610537686885 139.77757884514327 3.29262763</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0034_p2438_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2438_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2438_0">
											<gml:posList>35.538590905013415 139.77753618141264 4.87435293 35.53858737072828 139.77753861570488 4.87431841 35.5386070035875 139.77758128256684 4.87359309 35.538610537873396 139.7775788482759 4.87362761 35.538590905013415 139.77753618141264 4.87435293</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0034_p2437_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2437_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2437_0">
											<gml:posList>35.53860674524738 139.77758071991715 4.16860262 35.538588491823056 139.77754105089346 4.16927692 35.53857689436783 139.7775494322075 4.16915745 35.538594917864984 139.7775888062075 4.16848832 35.53856457528595 139.77761119754658 4.16816994 35.53860674524738 139.77758071991715 4.16860262</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0034_p2437_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2437_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2437_3">
											<gml:posList>35.538521502617186 139.77758960224375 4.16858944 35.53853078613844 139.77758275393307 4.16868597 35.538521454535356 139.77758949773715 4.16859122 35.538521502617186 139.77758960224375 4.16858944</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0034_p2434_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2434_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2434_1">
											<gml:posList>35.53849370647098 139.77761010950368 5.36330176 35.53851197946892 139.77765001311136 5.36262381 35.53859491800895 139.77758880857337 5.3634883 35.53857665783922 139.77754891754057 5.36416624 35.53849370647098 139.77761010950368 5.36330176</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2438_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2438_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2438_1">
											<gml:posList>35.53859090482195 139.77753617826951 3.29335295 35.538590905013415 139.77753618141264 4.87435293 35.538610537873396 139.7775788482759 4.87362761 35.538610537686885 139.77757884514327 3.29262763 35.53859090482195 139.77753617826951 3.29335295</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2438_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2438_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2438_2">
											<gml:posList>35.538610537873396 139.7775788482759 4.87362761 35.5386070035875 139.77758128256684 4.87359309 35.53860700340009 139.77758127943486 3.29259311 35.538610537686885 139.77757884514327 3.29262763 35.538610537873396 139.7775788482759 4.87362761</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2438_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2438_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2438_3">
											<gml:posList>35.538588491823056 139.77754105089346 4.16927692 35.53860674524738 139.77758071991715 4.16860262 35.538606745143454 139.77758071818175 3.29260263 35.53860700340009 139.77758127943486 3.29259311 35.5386070035875 139.77758128256684 4.87359309 35.53858737072828 139.77753861570488 4.87431841 35.538587370536 139.7775386125623 3.29331844 35.53858849171662 139.77754104915252 3.29327693 35.538588491823056 139.77754105089346 4.16927692</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2438_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2438_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2438_4">
											<gml:posList>35.53858737072828 139.77753861570488 4.87431841 35.538590905013415 139.77753618141264 4.87435293 35.53859090482195 139.77753617826951 3.29335295 35.538587370536 139.7775386125623 3.29331844 35.53858737072828 139.77753861570488 4.87431841</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2437_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2437_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2437_1">
											<gml:posList>35.538606745143454 139.77758071818175 3.29260263 35.53860674524738 139.77758071991715 4.16860262 35.53856457528595 139.77761119754658 4.16816994 35.538564575176174 139.77761119581533 3.29216996 35.538606745143454 139.77758071818175 3.29260263</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2437_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2437_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2437_2">
											<gml:posList>35.538588491823056 139.77754105089346 4.16927692 35.53858849171662 139.77754104915252 3.29327693 35.538576894259855 139.7775494304678 3.29315747 35.53857689436783 139.7775494322075 4.16915745 35.538588491823056 139.77754105089346 4.16927692</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2437_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2437_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2437_4">
											<gml:posList>35.538521502501546 139.77758960050951 3.29258945 35.538521502617186 139.77758960224375 4.16858944 35.538521454535356 139.77758949773715 4.16859122 35.538521454419715 139.77758949600295 3.29259123 35.538521502501546 139.77758960050951 3.29258945</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2437_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2437_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2437_5">
											<gml:posList>35.53853078613844 139.77758275393307 4.16868597 35.53853078602406 139.777582752198 3.29268598 35.538521454419715 139.77758949600295 3.29259123 35.538521454535356 139.77758949773715 4.16859122 35.53853078613844 139.77758275393307 4.16868597</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2434_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2434_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2434_0">
											<gml:posList>35.538594917864984 139.7775888062075 4.16848832 35.53857689436783 139.7775494322075 4.16915745 35.538576894259855 139.7775494304678 3.29315747 35.538576657583846 139.77754891342735 3.29316627 35.53857665783922 139.77754891754057 5.36416624 35.53859491800895 139.77758880857337 5.3634883 35.538594917864984 139.7775888062075 4.16848832</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2434_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2434_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2434_2">
											<gml:posList>35.53859491800895 139.77758880857337 5.3634883 35.53851197946892 139.77765001311136 5.36262381 35.53851197919248 139.7776500090309 3.29162384 35.538564575176174 139.77761119581533 3.29216996 35.53856457528595 139.77761119754658 4.16816994 35.538594917864984 139.7775888062075 4.16848832 35.53859491800895 139.77758880857337 5.3634883</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2434_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2434_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2434_3">
											<gml:posList>35.53851197946892 139.77765001311136 5.36262381 35.53849370647098 139.77761010950368 5.36330176 35.53849370618849 139.77761010541033 3.29230179 35.53851197919248 139.7776500090309 3.29162384 35.53851197946892 139.77765001311136 5.36262381</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0034_p2434_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0034_p2434_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0034_p2434_4">
											<gml:posList>35.53849370647098 139.77761010950368 5.36330176 35.53857665783922 139.77754891754057 5.36416624 35.538576657583846 139.77754891342735 3.29316627 35.53853078602406 139.777582752198 3.29268598 35.53853078613844 139.77758275393307 4.16868597 35.538521502617186 139.77758960224375 4.16858944 35.538521502501546 139.77758960050951 3.29258945 35.53849370618849 139.77761010541033 3.29230179 35.53849370647098 139.77761010950368 5.36330176</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:address>
				<core:Address>
					<core:xalAddress>
						<xAL:AddressDetails>
							<xAL:Country>
								<xAL:CountryName>日本</xAL:CountryName>
								<xAL:Locality>
									<xAL:LocalityName Type="Town">東京都大田区羽田空港二丁目</xAL:LocalityName>
								</xAL:Locality>
							</xAL:Country>
							<xAL:PostalCode>
								<xAL:PostalCodeNumber>123-4567</xAL:PostalCodeNumber>
							</xAL:PostalCode>
							<xAL:ThoroughFare>
								<xAL:ThoroughFareName>Test Street テスト丁目</xAL:ThoroughFareName>
								<xAL:ThoroughFareNumber>Test番地</xAL:ThoroughFareNumber>
							</xAL:ThoroughFare>
						</xAL:AddressDetails>
					</core:xalAddress>
				</core:Address>
			</bldg:address>
			<uro:buildingDetails>
				<uro:BuildingDetails>
					<uro:districtsAndZonesType codeSpace="../../codelists/Common_districtsAndZonesType.xml">11</uro:districtsAndZonesType>
					<uro:prefecture codeSpace="../../codelists/Common_prefecture.xml">13</uro:prefecture>
					<uro:city codeSpace="../../codelists/Common_localPublicAuthorities.xml">13111</uro:city>
				</uro:BuildingDetails>
			</uro:buildingDetails>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">2</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key2.xml">2</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">106</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key106.xml">20</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
		</bldg:Building>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<bldg:Building gml:id="BLD_365251b8-4a9c-4e13-b238-eae7a7fd9461">
			<gen:stringAttribute name="建物ID">
				<gen:value>13111-bldg-58</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="大字・町コード">
				<gen:value>42</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="町・丁目コード">
				<gen:value>2</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="13_区市町村コード_大字・町コード_町・丁目コード">
				<gen:value>13111042002</gen:value>
			</gen:stringAttribute>
			<gen:genericAttributeSet name="多摩水系多摩川、浅川、大栗川洪水浸水想定区域（想定最大規模）">
				<gen:stringAttribute name="規模">
					<gen:value>L2</gen:value>
				</gen:stringAttribute>
				<gen:stringAttribute name="浸水ランク">
					<gen:value>2</gen:value>
				</gen:stringAttribute>
				<gen:measureAttribute name="浸水深">
					<gen:value uom="m">0.760</gen:value>
				</gen:measureAttribute>
				<gen:measureAttribute name="継続時間">
					<gen:value uom="hour">0.00</gen:value>
				</gen:measureAttribute>
			</gen:genericAttributeSet>
			<bldg:measuredHeight uom="m">3.6</bldg:measuredHeight>
			<bldg:lod0RoofEdge>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>35.53959502469222 139.77509606766353 6.071 35.539488198974794 139.7751729811645 6.071 35.53953526186942 139.7752708133705 6.071 35.53964805171629 139.7751910988711 6.071 35.53962724435042 139.77514704808405 6.071 35.53980492898847 139.77501883672602 6.071 35.53975966062003 139.7749248408959 6.071 35.53957562490505 139.7750549979639 6.071 35.53959502469222 139.77509606766353 6.071</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</bldg:lod0RoofEdge>
			<bldg:lod1Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53959502469222 139.77509606766353 2.466 35.53957562490505 139.7750549979639 2.466 35.53975966062003 139.7749248408959 2.466 35.53980492898847 139.77501883672602 2.466 35.53962724435042 139.77514704808405 2.466 35.53964805171629 139.7751910988711 2.466 35.53953526186942 139.7752708133705 2.466 35.539488198974794 139.7751729811645 2.466 35.53959502469222 139.77509606766353 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53959502469222 139.77509606766353 2.466 35.539488198974794 139.7751729811645 2.466 35.539488198974794 139.7751729811645 6.071 35.53959502469222 139.77509606766353 6.071 35.53959502469222 139.77509606766353 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.539488198974794 139.7751729811645 2.466 35.53953526186942 139.7752708133705 2.466 35.53953526186942 139.7752708133705 6.071 35.539488198974794 139.7751729811645 6.071 35.539488198974794 139.7751729811645 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53953526186942 139.7752708133705 2.466 35.53964805171629 139.7751910988711 2.466 35.53964805171629 139.7751910988711 6.071 35.53953526186942 139.7752708133705 6.071 35.53953526186942 139.7752708133705 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53964805171629 139.7751910988711 2.466 35.53962724435042 139.77514704808405 2.466 35.53962724435042 139.77514704808405 6.071 35.53964805171629 139.7751910988711 6.071 35.53964805171629 139.7751910988711 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53962724435042 139.77514704808405 2.466 35.53980492898847 139.77501883672602 2.466 35.53980492898847 139.77501883672602 6.071 35.53962724435042 139.77514704808405 6.071 35.53962724435042 139.77514704808405 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53980492898847 139.77501883672602 2.466 35.53975966062003 139.7749248408959 2.466 35.53975966062003 139.7749248408959 6.071 35.53980492898847 139.77501883672602 6.071 35.53980492898847 139.77501883672602 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53975966062003 139.7749248408959 2.466 35.53957562490505 139.7750549979639 2.466 35.53957562490505 139.7750549979639 6.071 35.53975966062003 139.7749248408959 6.071 35.53975966062003 139.7749248408959 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53957562490505 139.7750549979639 2.466 35.53959502469222 139.77509606766353 2.466 35.53959502469222 139.77509606766353 6.071 35.53957562490505 139.7750549979639 6.071 35.53957562490505 139.7750549979639 2.466</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53959502469222 139.77509606766353 6.071 35.539488198974794 139.7751729811645 6.071 35.53953526186942 139.7752708133705 6.071 35.53964805171629 139.7751910988711 6.071 35.53962724435042 139.77514704808405 6.071 35.53980492898847 139.77501883672602 6.071 35.53975966062003 139.7749248408959 6.071 35.53957562490505 139.7750549979639 6.071 35.53959502469222 139.77509606766353 6.071</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod1Solid>
			<bldg:lod2Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_b_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0286_p2525_8"/>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod2Solid>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_eba92ee6-875a-49e3-af98-d87c46e886d2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_b_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_b_0">
											<gml:posList>35.53962724213174 139.77514704193166 2.61258556 35.53964805167645 139.77519109512446 2.61174215 35.53953526227407 139.77527080592006 2.61015184 35.53948819892041 139.77517297772658 2.61202916 35.539595027191616 139.7750960659937 2.61356192 35.5395756248703 139.77505499378375 2.61435517 35.53975966102497 139.77492484044808 2.61701926 35.53980492879746 139.77501883335722 2.61520901 35.53962724213174 139.77514704193166 2.61258556</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0286_p2525_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_0">
											<gml:posList>35.53975966082239 139.77492484818836 5.84601919 35.53957562476125 139.7750550014581 5.8433551 35.53959502707272 139.7750960736473 5.84256185 35.539488198855764 139.77517298534124 5.84102908 35.53953526218549 139.7752708134852 5.83915177 35.53964805153058 139.77519110273002 5.84074207 35.53962724199643 139.77514704955948 5.84158548 35.539804928571954 139.77501884104987 5.84420894 35.53975966082239 139.77492484818836 5.84601919</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0286_p2525_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_1">
											<gml:posList>35.539804928571954 139.77501884104987 5.84420894 35.53962724199643 139.77514704955948 5.84158548 35.53962724213174 139.77514704193166 2.61258556 35.53980492879746 139.77501883335722 2.61520901 35.539804928571954 139.77501884104987 5.84420894</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0286_p2525_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_2">
											<gml:posList>35.53962724199643 139.77514704955948 5.84158548 35.53964805153058 139.77519110273002 5.84074207 35.53964805167645 139.77519109512446 2.61174215 35.53962724213174 139.77514704193166 2.61258556 35.53962724199643 139.77514704955948 5.84158548</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0286_p2525_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_3">
											<gml:posList>35.53964805153058 139.77519110273002 5.84074207 35.53953526218549 139.7752708134852 5.83915177 35.53953526227407 139.77527080592006 2.61015184 35.53964805167645 139.77519109512446 2.61174215 35.53964805153058 139.77519110273002 5.84074207</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0286_p2525_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_4">
											<gml:posList>35.53953526218549 139.7752708134852 5.83915177 35.539488198855764 139.77517298534124 5.84102908 35.53948819892041 139.77517297772658 2.61202916 35.53953526227407 139.77527080592006 2.61015184 35.53953526218549 139.7752708134852 5.83915177</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0286_p2525_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_5">
											<gml:posList>35.539488198855764 139.77517298534124 5.84102908 35.53959502707272 139.7750960736473 5.84256185 35.539595027191616 139.7750960659937 2.61356192 35.53948819892041 139.77517297772658 2.61202916 35.539488198855764 139.77517298534124 5.84102908</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0286_p2525_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_6">
											<gml:posList>35.53959502707272 139.7750960736473 5.84256185 35.53957562476125 139.7750550014581 5.8433551 35.5395756248703 139.77505499378375 2.61435517 35.539595027191616 139.7750960659937 2.61356192 35.53959502707272 139.7750960736473 5.84256185</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0286_p2525_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_7">
											<gml:posList>35.53957562476125 139.7750550014581 5.8433551 35.53975966082239 139.77492484818836 5.84601919 35.53975966102497 139.77492484044808 2.61701926 35.5395756248703 139.77505499378375 2.61435517 35.53957562476125 139.7750550014581 5.8433551</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0286_p2525_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0286_p2525_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0286_p2525_8">
											<gml:posList>35.53975966082239 139.77492484818836 5.84601919 35.539804928571954 139.77501884104987 5.84420894 35.53980492879746 139.77501883335722 2.61520901 35.53975966102497 139.77492484044808 2.61701926 35.53975966082239 139.77492484818836 5.84601919</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:address>
				<core:Address>
					<core:xalAddress>
						<xAL:AddressDetails>
							<xAL:Country>
								<xAL:CountryName>日本</xAL:CountryName>
								<xAL:Locality>
									<xAL:LocalityName Type="Town">東京都大田区羽田空港二丁目</xAL:LocalityName>
								</xAL:Locality>
							</xAL:Country>
						</xAL:AddressDetails>
					</core:xalAddress>
				</core:Address>
			</bldg:address>
			<uro:buildingDetails>
				<uro:BuildingDetails>
					<uro:buildingRoofEdgeArea uom="m2">369.78030</uro:buildingRoofEdgeArea>
					<uro:districtsAndZonesType codeSpace="../../codelists/Common_districtsAndZonesType.xml">11</uro:districtsAndZonesType>
					<uro:prefecture codeSpace="../../codelists/Common_prefecture.xml">13</uro:prefecture>
					<uro:city codeSpace="../../codelists/Common_localPublicAuthorities.xml">13111</uro:city>
					<uro:surveyYear>2016</uro:surveyYear>
				</uro:BuildingDetails>
			</uro:buildingDetails>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">2</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key2.xml">2</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">106</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key106.xml">20</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
		</bldg:Building>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<bldg:Building gml:id="BLD_93fce453-1e4e-459e-b4ed-3fe18a78cbf4">
			<gen:stringAttribute name="建物ID">
				<gen:value>13111-bldg-63</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="大字・町コード">
				<gen:value>42</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="町・丁目コード">
				<gen:value>2</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="13_区市町村コード_大字・町コード_町・丁目コード">
				<gen:value>13111042002</gen:value>
			</gen:stringAttribute>
			<gen:genericAttributeSet name="多摩水系多摩川、浅川、大栗川洪水浸水想定区域（想定最大規模）">
				<gen:stringAttribute name="規模">
					<gen:value>L2</gen:value>
				</gen:stringAttribute>
				<gen:stringAttribute name="浸水ランク">
					<gen:value>1</gen:value>
				</gen:stringAttribute>
				<gen:measureAttribute name="浸水深">
					<gen:value uom="m">0.360</gen:value>
				</gen:measureAttribute>
				<gen:measureAttribute name="継続時間">
					<gen:value uom="hour">0.00</gen:value>
				</gen:measureAttribute>
			</gen:genericAttributeSet>
			<bldg:measuredHeight uom="m">3.6</bldg:measuredHeight>
			<bldg:lod0RoofEdge>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>35.5400102489919 139.7751977291029 6.464 35.53994779526228 139.77506548249312 6.464 35.53971358705839 139.77523135676995 6.464 35.53977598293244 139.77536363073668 6.464 35.5400102489919 139.7751977291029 6.464</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</bldg:lod0RoofEdge>
			<bldg:lod1Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.5400102489919 139.7751977291029 2.839 35.53977598293244 139.77536363073668 2.839 35.53971358705839 139.77523135676995 2.839 35.53994779526228 139.77506548249312 2.839 35.5400102489919 139.7751977291029 2.839</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.5400102489919 139.7751977291029 2.839 35.53994779526228 139.77506548249312 2.839 35.53994779526228 139.77506548249312 6.464 35.5400102489919 139.7751977291029 6.464 35.5400102489919 139.7751977291029 2.839</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53994779526228 139.77506548249312 2.839 35.53971358705839 139.77523135676995 2.839 35.53971358705839 139.77523135676995 6.464 35.53994779526228 139.77506548249312 6.464 35.53994779526228 139.77506548249312 2.839</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53971358705839 139.77523135676995 2.839 35.53977598293244 139.77536363073668 2.839 35.53977598293244 139.77536363073668 6.464 35.53971358705839 139.77523135676995 6.464 35.53971358705839 139.77523135676995 2.839</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53977598293244 139.77536363073668 2.839 35.5400102489919 139.7751977291029 2.839 35.5400102489919 139.7751977291029 6.464 35.53977598293244 139.77536363073668 6.464 35.53977598293244 139.77536363073668 2.839</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.5400102489919 139.7751977291029 6.464 35.53994779526228 139.77506548249312 6.464 35.53971358705839 139.77523135676995 6.464 35.53977598293244 139.77536363073668 6.464 35.5400102489919 139.7751977291029 6.464</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod1Solid>
			<bldg:lod2Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember xlink:href="#poly_HNAP0285_b_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0285_p2524_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0285_p2524_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0285_p2524_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0285_p2524_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0285_p2524_4"/>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod2Solid>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_e7498e75-e2b1-46e1-8f58-53b4e2282008">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0285_b_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0285_b_0">
											<gml:posList>35.539775982785116 139.77536362374337 2.98150642 35.5397135867804 139.77523135322355 2.98400399 35.53994779512094 139.77506548213535 2.98743908 35.54001024928995 139.77519772509544 2.98494216 35.539775982785116 139.77536362374337 2.98150642</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0285_p2524_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0285_p2524_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0285_p2524_0">
											<gml:posList>35.5399477948347 139.77506548950043 6.08843901 35.53971358660841 139.775231360508 6.08500392 35.539775982582704 139.77536363096368 6.08250635 35.5400102489732 139.7751977323963 6.08594209 35.5399477948347 139.77506548950043 6.08843901</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0285_p2524_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0285_p2524_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0285_p2524_1">
											<gml:posList>35.5400102489732 139.7751977323963 6.08594209 35.539775982582704 139.77536363096368 6.08250635 35.539775982785116 139.77536362374337 2.98150642 35.54001024928995 139.77519772509544 2.98494216 35.5400102489732 139.7751977323963 6.08594209</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0285_p2524_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0285_p2524_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0285_p2524_2">
											<gml:posList>35.539775982582704 139.77536363096368 6.08250635 35.53971358660841 139.775231360508 6.08500392 35.5397135867804 139.77523135322355 2.98400399 35.539775982785116 139.77536362374337 2.98150642 35.539775982582704 139.77536363096368 6.08250635</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0285_p2524_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0285_p2524_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0285_p2524_3">
											<gml:posList>35.53971358660841 139.775231360508 6.08500392 35.5399477948347 139.77506548950043 6.08843901 35.53994779512094 139.77506548213535 2.98743908 35.5397135867804 139.77523135322355 2.98400399 35.53971358660841 139.775231360508 6.08500392</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0285_p2524_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0285_p2524_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0285_p2524_4">
											<gml:posList>35.5399477948347 139.77506548950043 6.08843901 35.5400102489732 139.7751977323963 6.08594209 35.54001024928995 139.77519772509544 2.98494216 35.53994779512094 139.77506548213535 2.98743908 35.5399477948347 139.77506548950043 6.08843901</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:address>
				<core:Address>
					<core:xalAddress>
						<xAL:AddressDetails>
							<xAL:Country>
								<xAL:CountryName>日本</xAL:CountryName>
								<xAL:Locality>
									<xAL:LocalityName Type="Town">東京都大田区羽田空港二丁目</xAL:LocalityName>
								</xAL:Locality>
							</xAL:Country>
						</xAL:AddressDetails>
					</core:xalAddress>
				</core:Address>
			</bldg:address>
			<uro:buildingDetails>
				<uro:BuildingDetails>
					<uro:buildingRoofEdgeArea uom="m2">397.69065</uro:buildingRoofEdgeArea>
					<uro:districtsAndZonesType codeSpace="../../codelists/Common_districtsAndZonesType.xml">11</uro:districtsAndZonesType>
					<uro:prefecture codeSpace="../../codelists/Common_prefecture.xml">13</uro:prefecture>
					<uro:city codeSpace="../../codelists/Common_localPublicAuthorities.xml">13111</uro:city>
					<uro:surveyYear>2016</uro:surveyYear>
				</uro:BuildingDetails>
			</uro:buildingDetails>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">2</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key2.xml">2</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">106</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key106.xml">20</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
		</bldg:Building>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<bldg:Building gml:id="BLD_ae7f1207-dd09-45bc-8881-40533f3700bb">
			<gen:stringAttribute name="建物ID">
				<gen:value>13111-bldg-98</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="大字・町コード">
				<gen:value>42</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="町・丁目コード">
				<gen:value>2</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="13_区市町村コード_大字・町コード_町・丁目コード">
				<gen:value>13111042002</gen:value>
			</gen:stringAttribute>
			<gen:genericAttributeSet name="多摩水系多摩川、浅川、大栗川洪水浸水想定区域（想定最大規模）">
				<gen:stringAttribute name="規模">
					<gen:value>L2</gen:value>
				</gen:stringAttribute>
				<gen:stringAttribute name="浸水ランク">
					<gen:value>1</gen:value>
				</gen:stringAttribute>
				<gen:measureAttribute name="浸水深">
					<gen:value uom="m">0.080</gen:value>
				</gen:measureAttribute>
				<gen:measureAttribute name="継続時間">
					<gen:value uom="hour">0.00</gen:value>
				</gen:measureAttribute>
			</gen:genericAttributeSet>
			<bldg:measuredHeight uom="m">16.1</bldg:measuredHeight>
			<bldg:lod0RoofEdge>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>35.53830005697697 139.77849467736633 17.18 35.53862163103191 139.77917345181436 17.18 35.53862702956105 139.77916962913025 17.18 35.538670556329414 139.77926068928994 17.18 35.54042798701998 139.77800910244392 17.18 35.54038713094597 139.7779237077419 17.18 35.54038972790945 139.7779218564913 17.18 35.54038951226229 139.77792139013783 17.18 35.54023739221251 139.77758087632716 17.18 35.540053639138186 139.7772124167913 17.18 35.54005338108689 139.77721187327265 17.18 35.540051875355694 139.77721293966158 17.18 35.53998735549778 139.7770756650351 17.18 35.53998588851529 139.77707670493058 17.18 35.53997697218881 139.77705792435523 17.18 35.538213152192796 139.7783113522336 17.18 35.53822108847172 139.77832806764022 17.18 35.538218883480674 139.77832964175457 17.18 35.538297433861864 139.77849652525973 17.18 35.53830005697697 139.77849467736633 17.18</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</bldg:lod0RoofEdge>
			<bldg:lod1Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53830005697697 139.77849467736633 3.568 35.538297433861864 139.77849652525973 3.568 35.538218883480674 139.77832964175457 3.568 35.53822108847172 139.77832806764022 3.568 35.538213152192796 139.7783113522336 3.568 35.53997697218881 139.77705792435523 3.568 35.53998588851529 139.77707670493058 3.568 35.53998735549778 139.7770756650351 3.568 35.540051875355694 139.77721293966158 3.568 35.54005338108689 139.77721187327265 3.568 35.540053639138186 139.7772124167913 3.568 35.54023739221251 139.77758087632716 3.568 35.54038951226229 139.77792139013783 3.568 35.54038972790945 139.7779218564913 3.568 35.54038713094597 139.7779237077419 3.568 35.54042798701998 139.77800910244392 3.568 35.538670556329414 139.77926068928994 3.568 35.53862702956105 139.77916962913025 3.568 35.53862163103191 139.77917345181436 3.568 35.53830005697697 139.77849467736633 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53830005697697 139.77849467736633 3.568 35.53862163103191 139.77917345181436 3.568 35.53862163103191 139.77917345181436 17.18 35.53830005697697 139.77849467736633 17.18 35.53830005697697 139.77849467736633 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53862163103191 139.77917345181436 3.568 35.53862702956105 139.77916962913025 3.568 35.53862702956105 139.77916962913025 17.18 35.53862163103191 139.77917345181436 17.18 35.53862163103191 139.77917345181436 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53862702956105 139.77916962913025 3.568 35.538670556329414 139.77926068928994 3.568 35.538670556329414 139.77926068928994 17.18 35.53862702956105 139.77916962913025 17.18 35.53862702956105 139.77916962913025 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538670556329414 139.77926068928994 3.568 35.54042798701998 139.77800910244392 3.568 35.54042798701998 139.77800910244392 17.18 35.538670556329414 139.77926068928994 17.18 35.538670556329414 139.77926068928994 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54042798701998 139.77800910244392 3.568 35.54038713094597 139.7779237077419 3.568 35.54038713094597 139.7779237077419 17.18 35.54042798701998 139.77800910244392 17.18 35.54042798701998 139.77800910244392 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54038713094597 139.7779237077419 3.568 35.54038972790945 139.7779218564913 3.568 35.54038972790945 139.7779218564913 17.18 35.54038713094597 139.7779237077419 17.18 35.54038713094597 139.7779237077419 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54038972790945 139.7779218564913 3.568 35.54038951226229 139.77792139013783 3.568 35.54038951226229 139.77792139013783 17.18 35.54038972790945 139.7779218564913 17.18 35.54038972790945 139.7779218564913 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54038951226229 139.77792139013783 3.568 35.54023739221251 139.77758087632716 3.568 35.54023739221251 139.77758087632716 17.18 35.54038951226229 139.77792139013783 17.18 35.54038951226229 139.77792139013783 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54023739221251 139.77758087632716 3.568 35.540053639138186 139.7772124167913 3.568 35.540053639138186 139.7772124167913 17.18 35.54023739221251 139.77758087632716 17.18 35.54023739221251 139.77758087632716 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.540053639138186 139.7772124167913 3.568 35.54005338108689 139.77721187327265 3.568 35.54005338108689 139.77721187327265 17.18 35.540053639138186 139.7772124167913 17.18 35.540053639138186 139.7772124167913 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54005338108689 139.77721187327265 3.568 35.540051875355694 139.77721293966158 3.568 35.540051875355694 139.77721293966158 17.18 35.54005338108689 139.77721187327265 17.18 35.54005338108689 139.77721187327265 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.540051875355694 139.77721293966158 3.568 35.53998735549778 139.7770756650351 3.568 35.53998735549778 139.7770756650351 17.18 35.540051875355694 139.77721293966158 17.18 35.540051875355694 139.77721293966158 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53998735549778 139.7770756650351 3.568 35.53998588851529 139.77707670493058 3.568 35.53998588851529 139.77707670493058 17.18 35.53998735549778 139.7770756650351 17.18 35.53998735549778 139.7770756650351 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53998588851529 139.77707670493058 3.568 35.53997697218881 139.77705792435523 3.568 35.53997697218881 139.77705792435523 17.18 35.53998588851529 139.77707670493058 17.18 35.53998588851529 139.77707670493058 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53997697218881 139.77705792435523 3.568 35.538213152192796 139.7783113522336 3.568 35.538213152192796 139.7783113522336 17.18 35.53997697218881 139.77705792435523 17.18 35.53997697218881 139.77705792435523 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538213152192796 139.7783113522336 3.568 35.53822108847172 139.77832806764022 3.568 35.53822108847172 139.77832806764022 17.18 35.538213152192796 139.7783113522336 17.18 35.538213152192796 139.7783113522336 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53822108847172 139.77832806764022 3.568 35.538218883480674 139.77832964175457 3.568 35.538218883480674 139.77832964175457 17.18 35.53822108847172 139.77832806764022 17.18 35.53822108847172 139.77832806764022 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538218883480674 139.77832964175457 3.568 35.538297433861864 139.77849652525973 3.568 35.538297433861864 139.77849652525973 17.18 35.538218883480674 139.77832964175457 17.18 35.538218883480674 139.77832964175457 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.538297433861864 139.77849652525973 3.568 35.53830005697697 139.77849467736633 3.568 35.53830005697697 139.77849467736633 17.18 35.538297433861864 139.77849652525973 17.18 35.538297433861864 139.77849652525973 3.568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53830005697697 139.77849467736633 17.18 35.53862163103191 139.77917345181436 17.18 35.53862702956105 139.77916962913025 17.18 35.538670556329414 139.77926068928994 17.18 35.54042798701998 139.77800910244392 17.18 35.54038713094597 139.7779237077419 17.18 35.54038972790945 139.7779218564913 17.18 35.54038951226229 139.77792139013783 17.18 35.54023739221251 139.77758087632716 17.18 35.540053639138186 139.7772124167913 17.18 35.54005338108689 139.77721187327265 17.18 35.540051875355694 139.77721293966158 17.18 35.53998735549778 139.7770756650351 17.18 35.53998588851529 139.77707670493058 17.18 35.53997697218881 139.77705792435523 17.18 35.538213152192796 139.7783113522336 17.18 35.53822108847172 139.77832806764022 17.18 35.538218883480674 139.77832964175457 17.18 35.538297433861864 139.77849652525973 17.18 35.53830005697697 139.77849467736633 17.18</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod1Solid>
			<bldg:lod2Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_b_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2422_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2467_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2466_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2465_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2464_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2463_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2462_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2461_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2460_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2459_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2458_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2457_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2456_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2455_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2454_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2453_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2452_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2451_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2450_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_11"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_12"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_13"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_14"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_15"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_16"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_24"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2409_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2408_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2407_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2406_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2405_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2404_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2403_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2402_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2420_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2414_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2413_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2412_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2411_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2410_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2419_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2418_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2417_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2416_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2415_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2428_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2426_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2426_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2423_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2423_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2422_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2424_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2425_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2427_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2467_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2467_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2467_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2467_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2373_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2466_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2466_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2466_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2466_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2465_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2465_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2465_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2465_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2464_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2464_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2464_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2464_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2463_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2463_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2463_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2463_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2462_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2462_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2462_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2462_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2461_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2461_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2461_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2461_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2460_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2460_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2460_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2460_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2459_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2459_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2459_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2459_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2458_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2458_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2458_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2458_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2457_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2457_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2457_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2457_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2456_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2456_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2456_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2456_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2455_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2455_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2455_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2455_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2454_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2454_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2454_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2454_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2453_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2453_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2453_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2453_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2452_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2452_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2452_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2452_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2451_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2451_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2451_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2451_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2450_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2450_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2450_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2450_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2375_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_11"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2431_12"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_17"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_18"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_19"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_20"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_21"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_22"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_23"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_25"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_26"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_27"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_28"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_29"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_30"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_31"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_32"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_33"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_34"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_35"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2376_36"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2430_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2409_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2409_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2409_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2409_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2408_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2408_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2408_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2408_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2407_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2407_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2407_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2407_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2406_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2406_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2406_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2406_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2405_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2405_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2405_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2405_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2404_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2404_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2404_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2404_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2403_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2403_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2403_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2403_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2402_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2402_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2402_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2402_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2420_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2420_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2420_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2420_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2414_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2414_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2414_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2414_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2413_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2413_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2413_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2413_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2412_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2412_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2412_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2412_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2411_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2411_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2411_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2411_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2410_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2410_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2410_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2410_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2419_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2419_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2419_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2419_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2418_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2418_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2418_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2418_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2417_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2417_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2417_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2417_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2416_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2416_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2416_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2416_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2415_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2415_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2415_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2415_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2429_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2426_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2426_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2426_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2426_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2423_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2423_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2423_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2423_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2422_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2422_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2422_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2421_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2424_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2424_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2424_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2424_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2425_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2425_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2425_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2425_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2427_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2427_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2427_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0275_p2427_4"/>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod2Solid>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_1a7059fa-db0b-48e1-a1c4-c70d5ae441ae">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_b_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_b_0">
											<gml:posList>35.53862162926576 139.7791734306288 3.64726523 35.538300055547104 139.77849465658426 3.65778884 35.53829743229205 139.77849650539054 3.65776629 35.538218882331726 139.77832963201197 3.6604739 35.53898994825303 139.7777829238357 3.66791726 35.53899265792056 139.7777888342235 3.66782059 35.539071933775595 139.77773213764493 3.66868079 35.53907042598773 139.77772887769473 3.66873412 35.539081672993355 139.77772076914064 3.66885852 35.53908320513112 139.7777240765759 3.66880441 35.5391616241771 139.777667992623 3.66967366 35.5391592636296 139.7776628429107 3.66975791 35.539987355398786 139.7770756650125 3.67982534 35.54005187501377 139.77721293941875 3.67759398 35.54005338144049 139.7772118730159 3.67761388 35.54038972855478 139.77792184674163 3.66659483 35.53862162926576 139.7791734306288 3.64726523</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:OuterCeilingSurface gml:id="ceil_HNAP0275_p2375_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_8">
											<gml:posList>35.538670556550585 139.77926068640437 7.33295986 35.53862702543308 139.77916961763162 7.3343119 35.540387125242646 139.7779236962799 7.35356038 35.5404279867414 139.7780091005758 7.35229422 35.538670556550585 139.77926068640437 7.33295986</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:OuterCeilingSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:OuterCeilingSurface gml:id="ceil_HNAP0275_p2422_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2422_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2422_2">
											<gml:posList>35.53822109253841 139.77832807721822 9.83249294 35.538213152333384 139.77831135219768 9.8327671 35.53997697248547 139.77705792420045 9.85211373 35.53998589263471 139.77707671451665 9.85180589 35.53915926382513 139.77766285505885 9.84175781 35.53916162437037 139.7776680047662 9.84167356 35.53908320540053 139.77772408866477 9.84080431 35.539081673264214 139.7777207812328 9.84085842 35.53907042626949 139.77772888977893 9.84073402 35.53907193405592 139.77773214972606 9.84068069 35.53899265827783 139.77778884624985 9.83982049 35.538989948612915 139.7777829358678 9.83991716 35.53822109253841 139.77832807721822 9.83249294</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:OuterCeilingSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2467_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2467_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2467_0">
											<gml:posList>35.54005822338508 139.77771290712496 19.60638129 35.540077008016254 139.77769989691328 19.60661686 35.54006889387696 139.77768248729893 19.60688684 35.54005009961965 139.77769547708192 19.60665158 35.54005822338508 139.77771290712496 19.60638129</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2373_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_0">
											<gml:posList>35.54014979940239 139.777642642215 17.58764907 35.54007493069206 139.77769543537426 17.58668597 35.54007700824669 139.777699892901 17.56211646 35.5400582236093 139.77771290311875 17.56285561 35.54005622868957 139.77770862294545 17.58644764 35.539981104312126 139.777761596242 17.58549939 35.53998291690371 139.77776548998503 17.56404379 35.53996413234545 139.77777847825857 17.56487822 35.539962406769384 139.77777478063243 17.58526564 35.539887535084944 139.77782757560075 17.58433866 35.539889593220096 139.77783198923626 17.560008 35.539870800079704 139.7778449879217 17.56082858 35.53986883208877 139.77784076380073 17.58410935 35.53979369339117 139.77789374690596 17.5831972 35.53979574474562 139.77789815112635 17.5589254 35.539776951885806 139.77791113806362 17.55979832 35.53977499188047 139.77790693402238 17.58297244 35.53970012657066 139.77795972420765 17.58208168 35.53970242292044 139.7779646483392 17.55493635 35.539683621300426 139.77797764580964 17.55579543 35.53968141783948 139.77797291637955 17.58186134 35.539606271806115 139.77802590436843 17.5809854 35.53960856737973 139.7780308292573 17.55383916 35.53958976549244 139.7780438169742 17.5547439 35.53958756638461 139.77803909417054 17.58076962 35.539512700504815 139.7780918844697 17.57991502 35.539515238107064 139.77809732554596 17.54991935 35.539496436333565 139.77811031311305 17.55082874 35.53949399435705 139.77810507474805 17.57970374 35.53941886003109 139.77815805419255 17.57886422 35.53942139265985 139.77816348384837 17.54893046 35.53940259105528 139.7781764831761 17.5497949 35.53940015130944 139.77817124624977 17.57865743 35.53932528585377 139.7782240359617 17.57783899 35.53932805591906 139.77822997763656 17.5450865 35.539309263627345 139.7782429773658 17.54592647 35.539306583056614 139.7782372238055 17.57763679 35.539232552121554 139.77828942493227 17.57684524 35.539235430250905 139.77829559837522 17.54281511 35.539216628761004 139.7783086073514 17.54364784 35.53921384271798 139.7783026173988 17.57664743 35.53913897796091 139.77835540633197 17.57586494 35.53914210415436 139.77836211278924 17.53889789 35.53912330266919 139.7783750998756 17.53982586 35.53912027247077 139.7783685960029 17.57567169 35.53904513735018 139.77842157543137 17.57490451 35.53904825313942 139.7784282582451 17.53806596 35.53902946013057 139.77844126656865 17.53888539 35.539026432637264 139.77843476451832 17.57471579 35.538951564738284 139.77848755537894 17.57396942 35.538954928357406 139.77849477100654 17.53419545 35.53893612771237 139.77850776931686 17.53508311 35.53893286014696 139.77850074434429 17.57378521 35.5388577098993 139.77855373414988 17.57305417 35.53886106912411 139.77856093632695 17.53334884 35.538842278284214 139.77857393467974 17.53421209 35.5388390086363 139.77856692073232 17.57287451 35.5387641375337 139.77861971356376 17.57216428 35.53876773814014 139.77862743740755 17.52958863 35.538748947589376 139.77864043548576 17.53045653 35.53874543328232 139.77863290221745 17.5719891 35.53867030105389 139.77868587902842 17.57129452 35.538673890818345 139.77869359077565 17.52880071 35.53865509946053 139.7787065992111 17.52963233 35.53865159310609 139.77869907025246 17.57112384 35.53857672399374 139.77875186139238 17.57044977 35.538580560253976 139.77876010069434 17.52504626 35.53856176894591 139.77877308720628 17.52597313 35.53855802077939 139.77876504924296 17.57028364 35.538464869450735 139.77883073120412 17.56946963 35.54038972660206 139.77792186999406 15.72859464 35.54023739183786 139.77758087686195 17.58879419 35.54016850587175 139.77762945145815 17.58789197 35.540170342064485 139.77763339283965 17.56616974 35.540151548140784 139.77764639273877 17.56697446 35.54014979940239 139.777642642215 17.58764907</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2373_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_1">
											<gml:posList>35.5386216306677 139.77917345151639 15.70926508 35.54038972660206 139.77792186999406 15.72859464 35.538464869450735 139.77883073120412 17.56946963 35.5386216306677 139.77917345151639 15.70926508</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2373_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_2">
											<gml:posList>35.53893286014696 139.77850074434429 17.57378521 35.53892802129537 139.7784903410653 17.52205448 35.538946804140565 139.77847734277688 17.52315922 35.538951564738284 139.77848755537894 17.57396942 35.539026432637264 139.77843476451832 17.57471579 35.53902134537433 139.77842383857075 17.52037209 35.53904013737698 139.77841085118465 17.5215465 35.53904513735018 139.77842157543137 17.57490451 35.53912027247077 139.7783685960029 17.57567169 35.539115188332794 139.77835768345247 17.52138643 35.539133979989835 139.77834468425124 17.52252039 35.53913897796091 139.77835540633197 17.57586494 35.53921384271798 139.7783026173988 17.57664743 35.5392085224155 139.77829117860932 17.51976829 35.53922731475759 139.7782781908376 17.52095146 35.539232552121554 139.77828942493227 17.57684524 35.539306583056614 139.7782372238055 17.57763679 35.53930114800938 139.77822555782922 17.51960398 35.53931994096711 139.77821257118015 17.52079758 35.53932528585377 139.7782240359617 17.57783899 35.53940015130944 139.77817124624977 17.57865743 35.53939447570612 139.77815906329926 17.51805377 35.53941326867113 139.77814606674212 17.51921497 35.53941886003109 139.77815805419255 17.57886422 35.53949399435705 139.77810507474805 17.57970374 35.539488321235034 139.77809290490103 17.51915572 35.53950711418071 139.7780799061426 17.52031329 35.539512700504815 139.7780918844697 17.57991502 35.53958756638461 139.77803909417054 17.58076962 35.53958165028779 139.77802638850085 17.51757384 35.53960044338506 139.77801339990407 17.51877447 35.539606271806115 139.77802590436843 17.5809854 35.53968141783948 139.77797291637955 17.58186134 35.53967550616123 139.77796022749754 17.51874012 35.53969429934036 139.77794722843964 17.5199065 35.53970012657066 139.77795972420765 17.58208168 35.53977499188047 139.77790693402238 17.58297244 35.53976883660375 139.77789373120498 17.51728368 35.53978763057355 139.7778807298674 17.51844827 35.53979369339117 139.77789374690596 17.5831972 35.53986883208877 139.77784076380073 17.58410935 35.53986268499653 139.7778275693143 17.51847351 35.53988146998762 139.77781456881735 17.5196221 35.539887535084944 139.77782757560075 17.58433866 35.539962406769384 139.77777478063243 17.58526564 35.53995600857618 139.7777610700494 17.51703431 35.53997480254926 139.77774805872178 17.51817084 35.539981104312126 139.777761596242 17.58549939 35.54005622868957 139.77770862294545 17.58644764 35.54005009984591 139.7776954729875 17.52102682 35.54006889410917 139.77768248320217 17.52224935 35.54007493069206 139.77769543537426 17.58668597 35.54014979940239 139.777642642215 17.58764907 35.54014342541368 139.77762897158686 17.51963144 35.540162227082334 139.77761597374425 17.5208481 35.54016850587175 139.77762945145815 17.58789197 35.54023739183786 139.77758087686195 17.58879419 35.53828528733408 139.77846350225968 15.72029109 35.538464869450735 139.77883073120412 17.56946963 35.53855802077939 139.77876504924296 17.57028364 35.538553652521244 139.77875568134237 17.52367209 35.53857244594147 139.77874267308934 17.52474895 35.53857672399374 139.77875186139238 17.57044977 35.53865159310609 139.77869907025246 17.57112384 35.53864698310565 139.77868917134896 17.52188577 35.538665783288806 139.7786761735367 17.52302424 35.53867030105389 139.77868587902842 17.57129452 35.53874543328232 139.77863290221745 17.5719891 35.538740822379125 139.77862301812144 17.52280409 35.53875961388296 139.77861000948855 17.52388376 35.5387641375337 139.77861971356376 17.57216428 35.5388390086363 139.77856692073232 17.57287451 35.53883415404323 139.77855650663201 17.52106158 35.538852945735805 139.77854351960954 17.52222723 35.5388577098993 139.77855373414988 17.57305417 35.53893286014696 139.77850074434429 17.57378521</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2373_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_3">
											<gml:posList>35.54023739183786 139.77758087686195 17.58879419 35.54005338012617 139.7772118976093 15.73961368 35.53828528733408 139.77846350225968 15.72029109 35.54023739183786 139.77758087686195 17.58879419</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2373_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_4">
											<gml:posList>35.54023739183786 139.77758087686195 17.58879419 35.54038972660206 139.77792186999406 15.72859464 35.54005338012617 139.7772118976093 15.73961368 35.54023739183786 139.77758087686195 17.58879419</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2373_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_5">
											<gml:posList>35.5386216306677 139.77917345151639 15.70926508 35.538464869450735 139.77883073120412 17.56946963 35.53828528733408 139.77846350225968 15.72029109 35.5386216306677 139.77917345151639 15.70926508</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2466_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2466_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2466_0">
											<gml:posList>35.540151547886914 139.77764639676056 19.60759092 35.5401703418044 139.77763339686763 19.60783085 35.540162226819135 139.7776159778678 19.608101 35.54014342515652 139.77762897570813 19.60786109 35.540151547886914 139.77764639676056 19.60759092</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2465_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2465_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2465_0">
											<gml:posList>35.53940259104148 139.77817648710055 19.62857627 35.53942139263992 139.77816348777898 19.62878007 35.53941326865358 139.77814607073523 19.62905007 35.53939447569487 139.77815906728983 19.62884632 35.53940259104148 139.77817648710055 19.62857627</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2464_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2464_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2464_0">
											<gml:posList>35.5394964362893 139.77811031704954 19.62462255 35.539515238056595 139.77809732948867 19.62483069 35.53950711413216 139.7780799101479 19.62510073 35.5394883211927 139.7780929089039 19.62489243 35.5394964362893 139.77811031704954 19.62462255</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2463_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2463_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2463_0">
											<gml:posList>35.53958976541799 139.7780438209173 19.62069641 35.539608567299155 139.7780308332067 19.62090906 35.53960044330579 139.7780134039267 19.62117928 35.53958165021464 139.77802639252116 19.62096663 35.53958976541799 139.7780438209173 19.62069641</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2462_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2462_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2462_0">
											<gml:posList>35.53930926364418 139.77824298128326 19.63254763 35.53932805592977 139.7782299815603 19.63274692 35.53931994098064 139.77821257515572 19.63301673 35.53930114802912 139.7782255618023 19.63281764 35.53930926364418 139.77824298128326 19.63254763</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2461_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2461_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2461_0">
											<gml:posList>35.53977695175099 139.77791114203444 19.61790726 35.53979574460468 139.77789815510351 19.61812893 35.53978763043254 139.777880733929 19.61839906 35.53976883646878 139.77789373526417 19.61817717 35.53977695175099 139.77791114203444 19.61790726</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2460_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2460_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2460_0">
											<gml:posList>35.53968362119543 139.77797764977427 19.62178803 35.53970242280932 139.77796465231003 19.62200536 35.53969429922999 139.77794723248405 19.62227544 35.539675506057065 139.7779602315396 19.6220581 35.53968362119543 139.77797764977427 19.62178803</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2459_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2459_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2459_0">
											<gml:posList>35.539870799914965 139.77784499190435 19.61404386 35.539889593049125 139.77783199322522 19.61427023 35.539881469815974 139.7778145728909 19.61454036 35.53986268483102 139.7778275733854 19.61431397 35.539870799914965 139.77784499190435 19.61404386</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2458_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2458_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2458_0">
											<gml:posList>35.539964132151326 139.77777848224733 19.61020831 35.53998291670345 139.7777654939801 19.61043902 35.53997480234707 139.77774806281255 19.61070935 35.53995600838012 139.77776107413774 19.61047827 35.539964132151326 139.77777848224733 19.61020831</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2457_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2457_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2457_0">
											<gml:posList>35.53902946024023 139.7784412704566 19.64461505 35.539048253242875 139.7784282621391 19.64480097 35.53904013748405 139.77841085511545 19.64507075 35.53902134548771 139.77842384249914 19.64488514 35.53902946024023 139.7784412704566 19.64461505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2456_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2456_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2456_0">
											<gml:posList>35.53893612785348 139.77850777319728 19.64867638 35.5389549284924 139.77849477489323 19.64885764 35.538946804278986 139.77847734669015 19.64912771 35.538928021440015 139.7784903449759 19.64894645 35.53893612785348 139.77850777319728 19.64867638</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2455_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2455_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2455_0">
											<gml:posList>35.53912330274767 139.778375103776 19.64057092 35.53914210422663 139.77836211669614 19.64076103 35.539133980065365 139.7783446881949 19.64103115 35.539115188414634 139.77835768739345 19.64084084 35.53912330274767 139.778375103776 19.64057092</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2454_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2454_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2454_0">
											<gml:posList>35.53921662880848 139.7783086112592 19.63655462 35.539235430292166 139.77829560228912 19.63674958 35.53922731480195 139.77827819479867 19.63701939 35.53920852246616 139.77829118256787 19.63682476 35.53921662880848 139.7783086112592 19.63655462</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2453_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2453_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2453_0">
											<gml:posList>35.538748947794026 139.77864043934545 19.6568724 35.53876773833857 139.77862744127358 19.65704463 35.53875961408465 139.7786100133712 19.65731467 35.53874082258712 139.77862302200145 19.65714229 35.538748947794026 139.77864043934545 19.6568724</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2452_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2452_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2452_0">
											<gml:posList>35.5388422784566 139.77857393853816 19.64776586 35.538861069290284 139.77856094019154 19.6479426 35.538852945905596 139.77854352350084 19.64821249 35.53883415421932 139.77855651052076 19.64803592 35.5388422784566 139.77857393853816 19.64776586</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2451_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2451_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2451_0">
											<gml:posList>35.53867389104869 139.77869359462852 19.66117508 35.53866578352249 139.7786761774063 19.66144493 35.53864698334572 139.77868917521593 19.66127724 35.53865509969699 139.77870660305777 19.66100722 35.53867389104869 139.77869359462852 19.66117508</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2450_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2450_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2450_0">
											<gml:posList>35.53858056051595 139.7787601045302 19.66032217 35.538572446206324 139.77874267693204 19.66059215 35.5385536527924 139.77875568518243 19.66042881 35.538561769214105 139.77877309103584 19.66015914 35.53858056051595 139.7787601045302 19.66032217</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2375_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_0">
											<gml:posList>35.54038088407035 139.77792811460156 7.503478 35.53863326663344 139.77916520005996 7.48436602 35.53867314817338 139.7792486337644 7.48312679 35.54041811254345 139.7780059255325 7.50232389 35.54038088407035 139.77792811460156 7.503478</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2375_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_4">
											<gml:posList>35.53867314833553 139.77924863634433 8.98312677 35.538633266805064 139.77916520265939 8.984366 35.53862702562339 139.7791696204899 8.98431188 35.538670556729706 139.77926068923915 8.98295984 35.54042798646432 139.77800910373406 9.00229419 35.540387124976206 139.77792369946013 9.00356036 35.54038088382966 139.77792811749174 9.00347797 35.54041811229392 139.77800592840438 9.00232386 35.53867314833553 139.77924863634433 8.98312677</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2431_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_0">
											<gml:posList>35.539210987794114 139.77772257036057 14.460776 35.539257211142925 139.7776905997983 14.46128038 35.53927532936196 139.77772980198682 14.4606449 35.53922372665938 139.7777654957545 14.46008214 35.53923044529472 139.77777987351197 14.4598497 35.539292550696054 139.77773762393684 14.46051644 35.539278566481855 139.77770821617406 14.46099222 35.53965718549625 139.77743774307908 14.4654802 35.53965180140265 139.77742456519388 14.46569437 35.53927440441112 139.77769418028356 14.4612194 35.53926274621266 139.7776696866662 14.46161734 35.53920500538257 139.77770963329098 14.4609863 35.539210987794114 139.77772257036057 14.460776</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_0">
											<gml:posList>35.53995741248193 139.77725514727965 13.48876985 35.53994826172322 139.7772343596839 13.48910703 35.539790276731125 139.7773462462134 13.48708669 35.539798833600535 139.77736724616034 13.48674568 35.539769484303115 139.7773880406887 13.48637738 35.539761148775746 139.77736762342914 13.48670883 35.539605000502036 139.77747820858895 13.48478717 35.539614190596 139.777500718429 13.48442222 35.539584828859276 139.77752152480713 13.48406779 35.53957565582234 139.77749899061388 13.48443307 35.53941694424776 139.77761139050702 13.48255642 35.53942622147174 139.77763414607054 13.482188 35.539396869717876 139.77765493727404 13.48184807 35.53938758342328 139.7776321838519 13.48221637 35.539324809749 139.77767664012725 13.48149683 35.539333850224274 139.77769881460688 13.48113806 35.539304498642046 139.77771961950822 13.4808049 35.53929521227595 139.77769685240642 13.48117319 35.5392787182515 139.77770853343898 13.48098706 35.53929255068552 139.77773762201946 13.48051646 35.53923044527465 139.77777987160098 13.47984972 35.53922372663831 139.77776549384131 13.48008215 35.53927532934882 139.77772980006824 13.48064491 35.53926875993461 139.7777155858948 13.48087503 35.53925640217812 139.777724337625 13.48073636 35.53925983148712 139.77773275290937 13.48060017 35.53923048202452 139.7777535472717 13.48027229 35.53922704466998 139.77774512852156 13.4804085 35.53921058394814 139.77775678594608 13.48022565 35.539227032088164 139.77779266892338 13.47964548 35.53904524488631 139.77792158888465 13.47766997 35.539038735456444 139.777907390317 13.47789921 35.539009783754395 139.77792790927498 13.47759266 35.539017843949445 139.7779449405411 13.47731757 35.5400456329009 139.77721737747302 13.48951144 35.54002656917026 139.77717681668992 13.49016738 35.539976917929465 139.7772119863115 13.48951525 35.53998675900735 139.77723434452767 13.48915252 35.53995741248193 139.77725514727965 13.48876985</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_1">
											<gml:posList>35.53900025813189 139.77793466037164 13.47749228 35.53899018331734 139.777912621296 13.47784875 35.53876079675686 139.7780750450012 13.47550436 35.53877008087879 139.77809779337932 13.47513784 35.538929236982945 139.77798499514776 13.47675122 35.53892047442387 139.77796351099005 13.47709775 35.53894983411897 139.7779427118261 13.47740246 35.53895859555891 139.7779641878745 13.47705598 35.53900025813189 139.77793466037164 13.47749228</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_2">
											<gml:posList>35.53900279217395 139.7779199975365 13.47772287 35.53900862228329 139.77792545507643 13.47763235 35.539009783754395 139.77792790927498 13.47759266 35.539038735456444 139.777907390317 13.47789921 35.53902864727748 139.77788538568424 13.47825516 35.538997116972034 139.77790771170686 13.47792173 35.53900279217395 139.7779199975365 13.47772287</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_3">
											<gml:posList>35.538963893537876 139.77785511137816 13.47878281 35.538962355166845 139.77785174613462 13.47883764 35.53872379978932 139.77802603664344 13.47631747 35.538725382183046 139.77802961522806 13.47625928 35.53890310933527 139.77789958349612 13.47812551 35.538911919659675 139.7778931375205 13.47822018 35.538963893537876 139.77785511137816 13.47878281</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_4">
											<gml:posList>35.53876079675686 139.7780750450012 13.47550436 35.53899018331734 139.777912621296 13.47784875 35.538963893537876 139.77785511137816 13.47878281 35.538911919659675 139.7778931375205 13.47822018 35.538927075156856 139.77792741269815 13.47766335 35.53891826482619 139.77793385861435 13.4775687 35.53890310933527 139.77789958349612 13.47812551 35.538725382183046 139.77802961522806 13.47625928 35.53873895542833 139.77806031114858 13.475761 35.538730146848756 139.77806674671092 13.47567083 35.53874024955288 139.77808959398268 13.47530099 35.53876079675686 139.7780750450012 13.47550436</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_5">
											<gml:posList>35.53898739731333 139.7778164165857 13.47938301 35.53899455538887 139.77781102355937 13.47946414 35.538987237436544 139.77779506147044 13.47972487 35.53822872105091 139.77833287159189 13.47240274 35.53824675545464 139.77837118423795 13.47177737 35.53834321106328 139.77830074694594 13.47265743 35.5383426216434 139.778299444612 13.4726786 35.538352626200414 139.77829215485008 13.47277104 35.53835321013021 139.77829344505537 13.47275006 35.538529517243994 139.7781646951642 13.47442639 35.53853833207631 139.7781582580344 13.47451235 35.53896127981299 139.7778493937584 13.47887598 35.538954548771336 139.77783466936054 13.47911617 35.538980419628466 139.7778171318906 13.47937666 35.53898739731333 139.7778164165857 13.47938301</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_6">
											<gml:posList>35.539001481695834 139.77793733696947 13.47744904 35.53900025813189 139.77793466037164 13.47749228 35.53895859555891 139.7779641878745 13.47705598 35.5389591196021 139.77796547240993 13.47703528 35.53892975689176 139.7779862698675 13.47673068 35.538929236982945 139.77798499514776 13.47675122 35.53877008087879 139.77809779337932 13.47513784 35.53874072032905 139.7781186019371 13.47484736 35.5387325493358 139.7780985468507 13.47517035 35.53857626773067 139.77820892134463 13.47366723 35.538585839296196 139.7782294949014 13.4733345 35.53855518085425 139.77825116317644 13.47304678 35.53854561024865 139.77823057325207 13.47337976 35.53838919795026 139.7783410394471 13.47195082 35.53839932445474 139.77836279943352 13.47159895 35.539001481695834 139.77793733696947 13.47744904</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_7">
											<gml:posList>35.53828897889179 139.77846088471628 13.47032306 35.539008108047796 139.77795183239894 13.4772151 35.539001481695834 139.77793733696947 13.47744904 35.53839932445474 139.77836279943352 13.47159895 35.538368669036366 139.77838445924047 13.47132615 35.53835853566268 139.77836269463037 13.47167809 35.53838919795026 139.7783410394471 13.47195082 35.53854561024865 139.77823057325207 13.47337976 35.5385450599903 139.77822938944263 13.47339892 35.53857570956862 139.77820772160615 13.47368666 35.53857626773067 139.77820892134463 13.47366723 35.5387325493358 139.7780985468507 13.47517035 35.53873144244816 139.77809583007948 13.47521415 35.53874024955288 139.77808959398268 13.47530099 35.538730146848756 139.77806674671092 13.47567083 35.538714991205524 139.7780324722434 13.47622728 35.53872379978932 139.77802603664344 13.47631747 35.538962355166845 139.77785174613462 13.47883764 35.53896127981299 139.7778493937584 13.47887598 35.53853833207631 139.7781582580344 13.47451235 35.53855348780946 139.77819253180982 13.47395627 35.53854467299607 139.77819896893874 13.47387035 35.538529517243994 139.7781646951642 13.47442639 35.53835321013021 139.77829344505537 13.47275006 35.5383672990796 139.77832457490305 13.47224478 35.53835729452812 139.77833186462908 13.47215237 35.53834321106328 139.77830074694594 13.47265743 35.53824675545464 139.77837118423795 13.47177737 35.53828897889179 139.77846088471628 13.47032306</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_8">
											<gml:posList>35.53899233039534 139.7778274812822 13.47920263 35.53898536166496 139.77782818664184 13.47919638 35.53896629662739 139.77784099107444 13.47900616 35.53896901656117 139.77784687924787 13.47891015 35.539000461194334 139.7778239054461 13.47925403 35.538999596384954 139.7778220191024 13.47928478 35.53899233039534 139.7778274812822 13.47920263</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_9">
											<gml:posList>35.53902864727748 139.77788538568424 13.47825516 35.539000461194334 139.7778239054461 13.47925403 35.53896901656117 139.77784687924787 13.47891015 35.538997116972034 139.77790771170686 13.47792173 35.53902864727748 139.77788538568424 13.47825516</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_10">
											<gml:posList>35.53929521227595 139.77769685240642 13.48117319 35.53932457292458 139.77767605924456 13.48150624 35.539324809749 139.77767664012725 13.48149683 35.53938758342328 139.7776321838519 13.48221637 35.53941694424776 139.77761139050702 13.48255642 35.53957565582234 139.77749899061388 13.48443307 35.53957556057053 139.77749875662133 13.48443687 35.539604899154966 139.77747796035376 13.4847912 35.539605000502036 139.77747820858895 13.48478717 35.539761148775746 139.77736762342914 13.48670883 35.539760261671276 139.777365450533 13.48674415 35.53953791338788 139.77752294558144 13.4840263 35.539278566469164 139.77770821425207 13.48099223 35.5392787182515 139.77770853343898 13.48098706 35.53929521227595 139.77769685240642 13.48117319</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_11">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_11">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_11">
											<gml:posList>35.53978956025446 139.77734448786384 13.48711528 35.539790276731125 139.7773462462134 13.48708669 35.53994826172322 139.7772343596839 13.48910703 35.539947565331964 139.77723277770642 13.48913272 35.539976917929465 139.7772119863115 13.48951525 35.54002656917026 139.77717681668992 13.49016738 35.53999870973325 139.77711754188448 13.49113103 35.5399428747117 139.77716165378763 13.49032132 35.53995987841444 139.77720252366584 13.48965392 35.539949423266876 139.7772107777999 13.48950343 35.5399324135746 139.77716991849675 13.49017056 35.53957651587542 139.7774510900979 13.48522036 35.53958249748588 139.7774644184685 13.48500363 35.539572648881744 139.77747157533216 13.48488186 35.53955214132318 139.7774258799866 13.4856261 35.53937583945698 139.77755399526347 13.48348854 35.53939421634771 139.7775949413037 13.48282196 35.539384367528214 139.77760209828256 13.48270497 35.53936385943126 139.77755640316678 13.48344901 35.53935384435446 139.77756366507532 13.48333054 35.539374173737734 139.7776089612659 13.48259301 35.53936430705615 139.7776161183476 13.48247653 35.53934382178563 139.77757043509982 13.48322035 35.53928228436174 139.77761515298934 13.48249815 35.53930172261164 139.77765844318307 13.48179339 35.53929186879265 139.77766560997637 13.48167859 35.539271365358495 139.77761992700297 13.48242237 35.53917871241535 139.7776872556707 13.48135607 35.53921058394814 139.77775678594608 13.48022565 35.53922704466998 139.77774512852156 13.4804085 35.53922119003897 139.77773078939634 13.48064077 35.53925055737596 139.77770999489462 13.48096873 35.53925640217812 139.777724337625 13.48073636 35.53926875993461 139.7777155858948 13.48087503 35.53925721112698 139.77769059787366 13.48128039 35.53921098777106 139.77772256844077 13.48077602 35.5392050053586 139.77770963136933 13.48098631 35.53926274619752 139.77766968473838 13.48161736 35.53927440439789 139.77769417835947 13.48121941 35.53965180144753 139.77742456322838 13.48569439 35.53965718554194 139.77743774111565 13.48548021 35.53953791338788 139.77752294558144 13.4840263 35.539760261671276 139.777365450533 13.48674415 35.53976019308321 139.77736528253172 13.48674688 35.53978956025446 139.77734448786384 13.48711528</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_12">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_12">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_12">
											<gml:posList>35.539984710380736 139.7770877563647 13.49161754 35.53916903029764 139.77766613339162 13.48170109 35.53917871241535 139.7776872556707 13.48135607 35.539271365358495 139.77761992700297 13.48242237 35.539281214335965 139.77761276997688 13.48253704 35.53998954297799 139.77709803836734 13.49144942 35.539984710380736 139.7770877563647 13.49161754</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_13">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_13">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_13">
											<gml:posList>35.53928228436174 139.77761515298934 13.48249815 35.53934382178563 139.77757043509982 13.48322035 35.53935367068287 139.77756327811565 13.48333686 35.53999065024457 139.77710039422678 13.49141093 35.53998954297799 139.77709803836734 13.49144942 35.539281214335965 139.77761276997688 13.48253704 35.53928228436174 139.77761515298934 13.48249815</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_14">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_14">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_14">
											<gml:posList>35.53935384435446 139.77756366507532 13.48333054 35.53936385943126 139.77755640316678 13.48344901 35.53937371337727 139.77754925809165 13.48356584 35.539991183117266 139.7771015279855 13.49139241 35.53999065024457 139.77710039422678 13.49141093 35.53935367068287 139.77756327811565 13.48333686 35.53935384435446 139.77756366507532 13.48333054</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_15">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_15">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_15">
											<gml:posList>35.53937583945698 139.77755399526347 13.48348854 35.53955214132318 139.7774258799866 13.4856261 35.53956198996882 139.77741872313996 13.48574791 35.53999304192158 139.77710548284375 13.49132781 35.539991183117266 139.7771015279855 13.49139241 35.53937371337727 139.77754925809165 13.48356584 35.53937583945698 139.77755399526347 13.48348854</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_16">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_16">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_16">
											<gml:posList>35.53957651587542 139.7774510900979 13.48522036 35.5399324135746 139.77716991849675 13.49017056 35.5399428747117 139.77716165378763 13.49032132 35.53999870973325 139.77711754188448 13.49113103 35.53999304192158 139.77710548284375 13.49132781 35.53956198996882 139.77741872313996 13.48574791 35.53957651587542 139.7774510900979 13.48522036</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2376_24">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_24">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_24">
											<gml:posList>35.538989948895164 139.77778294530313 14.67991709 35.538218884309465 139.77832965253637 14.67247374 35.538297434133696 139.77849652562722 14.66976614 35.53830005738425 139.77849467682412 14.66978868 35.53828528715652 139.7784635003247 14.67029111 35.53828897909408 139.77846088692814 14.67032304 35.538228721264566 139.7783328738279 14.67240272 35.53898723750702 139.7777950638075 14.67972485 35.539045244945875 139.77792159119787 14.67766995 35.5392270321134 139.7777926712609 14.67964546 35.5391690303338 139.77766613575284 14.68170108 35.539984710263 139.77708775883468 14.69161752 35.5400456327716 139.77721737991865 14.68951142 35.54005187381654 139.77721296186945 14.68959379 35.53998735431328 139.7770756876999 14.69182514 35.539159263978426 139.77766286458527 14.68175773 35.53921726601509 139.77778940062865 14.67970048 35.53913558670581 139.77784732609396 14.67880359 35.539047956032704 139.77790947200396 14.67786052 35.538989948895164 139.77778294530313 14.67991709</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2430_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_0">
											<gml:posList>35.539008108111005 139.77795183459122 14.61721509 35.53901784401095 139.77794494273454 14.61731755 35.5390086223464 139.77792545727343 14.61763233 35.53900279223815 139.77791999973437 14.61772285 35.53896629669809 139.77784099328645 14.61900614 35.53898536173232 139.77782818885615 14.61919637 35.53899233046144 139.77782748349665 14.61920262 35.53899959644961 139.7778220213178 14.61928476 35.53899455545452 139.77781102577677 14.61946412 35.53898739738033 139.7778164188021 14.61938299 35.53898041969665 139.7778171341069 14.61937665 35.53895454884419 139.77783467157377 14.61911615 35.539008108111005 139.77795183459122 14.61721509</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2409_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2409_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2409_0">
											<gml:posList>35.5387314427208 139.77809583531513 16.23021411 35.538740720597715 139.77811860716298 16.22984732 35.53877008113475 139.7780977986141 16.2301378 35.538760797016785 139.7780750502459 16.23050431 35.5387314427208 139.77809583531513 16.23021411</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2408_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2408_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2408_0">
											<gml:posList>35.53892047461423 139.7779635162733 16.22709771 35.53892975707815 139.77798627514085 16.22673064 35.53895911977568 139.7779654776923 16.22703523 35.538949834296716 139.77794271711832 16.22740242 35.53892047461423 139.7779635162733 16.22709771</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2407_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2407_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2407_0">
											<gml:posList>35.53890310953277 139.77789958879725 16.22312546 35.538918265017195 139.7779338639008 16.22256866 35.53892707534407 139.77792741798737 16.22266331 35.53891191985339 139.77789314282438 16.22322014 35.53890310953277 139.77789958879725 16.22312546</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2406_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2406_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2406_0">
											<gml:posList>35.53871499148476 139.7780324774969 16.22622723 35.538730147121484 139.77806675194978 16.22567079 35.538738955697184 139.7780603163902 16.22576096 35.538723800064766 139.77802604189972 16.22631743 35.53871499148476 139.7780324774969 16.22622723</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2405_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2405_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2405_0">
											<gml:posList>35.53852951760415 139.77816470037024 16.22942635 35.53854467334963 139.77819897413 16.22887031 35.538553488159316 139.77819253700383 16.22895623 35.538538332432665 139.77815826324323 16.2295123 35.53852951760415 139.77816470037024 16.22942635</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2404_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2404_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2404_0">
											<gml:posList>35.53834262205491 139.77829944941416 16.04267857 35.53835729493377 139.7783318694182 16.04215233 35.53836729948119 139.77832457969515 16.04224474 35.53835262660788 139.7782921596552 16.042771 35.53834262205491 139.77829944941416 16.04267857</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2403_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2403_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2403_0">
											<gml:posList>35.53857570987915 139.77820772634178 15.98868662 35.538545060312906 139.77822939416964 15.98839889 35.53855518117289 139.77825116789484 15.98804675 35.538585839602675 139.77822949962842 15.98833447 35.53857570987915 139.77820772634178 15.98868662</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2402_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2402_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2402_0">
											<gml:posList>35.53838919833531 139.77834104413944 15.99195078 35.5383585360599 139.7783626993142 15.99167805 35.53836866942961 139.7783844639157 15.99132611 35.538399324835815 139.77836280411728 15.99159892 35.53838919833531 139.77834104413944 15.99195078</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2420_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2420_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2420_0">
											<gml:posList>35.53922119009895 139.77773079475043 16.21564073 35.53923048208053 139.77775355261605 16.21527225 35.53925983153052 139.77773275826257 16.21560013 35.53925055742331 139.77771000025766 16.21596869 35.53922119009895 139.77773079475043 16.21564073</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2414_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2414_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2414_0">
											<gml:posList>35.53994756504626 139.77723278400666 16.58413267 35.53995741219145 139.7772551535692 16.5837698 35.539986758702554 139.77723435082714 16.58415247 35.539976917629446 139.7772119926218 16.58451519 35.53994756504626 139.77723278400666 16.58413267</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2413_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2413_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2413_0">
											<gml:posList>35.53976019291223 139.77736528801225 16.20674683 35.539769484128165 139.7773880461594 16.20637734 35.53979883341306 139.77736725164 16.20674564 35.539789560070965 139.7773444933532 16.20711523 35.53976019291223 139.77736528801225 16.20674683</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2412_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2412_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2412_0">
											<gml:posList>35.53960489905031 139.7774779657963 16.20979116 35.539575560478404 139.7774987620549 16.20943682 35.53958482876317 139.777521530231 16.20906774 35.53961419048737 139.77750072386175 16.20942217 35.53960489905031 139.7774779657963 16.20979116</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2411_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2411_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2411_0">
											<gml:posList>35.53941694422309 139.77761139601117 16.26755637 35.53938758341139 139.7776321893469 16.26721633 35.539396869702024 139.77765494275909 16.26684803 35.539426221443 139.77763415156465 16.26718795 35.53941694422309 139.77761139601117 16.26755637</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2410_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2410_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2410_0">
											<gml:posList>35.53929521230463 139.77769685788303 16.27117314 35.53930449866674 139.7777196249749 16.27080485 35.53933385023608 139.7776988200825 16.27113802 35.539324572940444 139.77767606473023 16.2715062 35.53929521230463 139.77769685788303 16.27117314</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2419_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2419_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2419_0">
											<gml:posList>35.53927136540149 139.77761993304642 16.54242232 35.539291868825806 139.77766561599802 16.54167855 35.539301722640026 139.7776584492081 16.54179334 35.53928121437418 139.77761277602377 16.54253699 35.53927136540149 139.77761993304642 16.54242232</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2418_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2418_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2418_0">
											<gml:posList>35.53935367068632 139.77756328417638 16.53833681 35.53934382179364 139.7775704411571 16.5382203 35.53936430705442 139.77761612438303 16.53747649 35.53937417373123 139.77760896730476 16.53759296 35.53935367068632 139.77756328417638 16.53833681</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2417_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2417_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2417_0">
											<gml:posList>35.53936385942974 139.7775564092307 16.53844896 35.53938436751686 139.77760210432476 16.53770492 35.53939421633157 139.77759494734917 16.53782191 35.53937371337098 139.777549264159 16.53856579 35.53936385942974 139.7775564092307 16.53844896</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2416_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2416_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2416_0">
											<gml:posList>35.53955214123127 139.77742588610295 16.53562605 35.539572648780094 139.77747158142677 16.53488181 35.539582497379456 139.77746442456655 16.53500358 35.53956198987223 139.77741872925978 16.53574786 35.53955214123127 139.77742588610295 16.53562605</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2415_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2415_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2415_0">
											<gml:posList>35.53993241336409 139.7771699232832 15.83017052 35.53994942305015 139.77721078257122 15.8295034 35.53995987819393 139.77720252844026 15.82965387 35.53994287449742 139.77716165857714 15.83032128 35.53993241336409 139.7771699232832 15.83017052</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2429_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_0">
											<gml:posList>35.539070426502704 139.7777288997839 14.95073394 35.539077279571295 139.77774371663307 14.9504917 35.539086666546076 139.77773694906247 14.9505955 35.53908853456961 139.77773560230457 14.95061618 35.5390816734885 139.7777207912443 14.95085834 35.539070426502704 139.7777288997839 14.95073394</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_0">
											<gml:posList>35.53911406271177 139.77779893067492 13.74606325 35.53911288717887 139.77779974721085 13.48624398 35.53908472231455 139.7777383478354 13.47778818 35.539085887349955 139.77773750841547 13.7376043 35.53911406271177 139.77779893067492 13.74606325</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_1">
											<gml:posList>35.53913258662366 139.77784945238557 14.01246165 35.53908239228874 139.7777400287081 13.99738757 35.53908122725315 139.77774086812795 13.73757147 35.53910936062074 139.77780219884144 13.74601782 35.53911007786231 139.77780170063895 13.90454435 35.53912178695962 139.77782727769875 13.90412979 35.53912108754814 139.7778277635643 13.74954043 35.53913141684275 139.77785028146127 13.75264412 35.53913258662366 139.77784945238557 14.01246165</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_2">
											<gml:posList>35.53912108754814 139.7778277635643 13.74954043 35.53912226303359 139.7778269459578 13.48974387 35.539132586604985 139.77784945137805 13.49284584 35.53913141684275 139.77785028146127 13.75264412 35.53912108754814 139.7778277635643 13.74954043</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_3">
											<gml:posList>35.53912226303359 139.7778269459578 13.48974387 35.53912461402403 139.777825311754 13.48976656 35.5391349261482 139.77784779221903 13.4928651 35.539132586604985 139.77784945137805 13.49284584 35.53912226303359 139.7778269459578 13.48974387</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_4">
											<gml:posList>35.53912461402403 139.777825311754 13.48976656 35.53912578952886 139.7778244951569 13.74958582 35.539136095928995 139.77784696314336 13.75268263 35.5391349261482 139.77784779221903 13.4928651 35.53912461402403 139.777825311754 13.48976656</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_5">
											<gml:posList>35.53912509011407 139.77782498163347 13.90416496 35.53911334546807 139.77779942950485 13.90457914 35.53911406271177 139.77779893067492 13.74606325 35.539085887349955 139.77773750841547 13.7376043 35.539084722337016 139.77773834885195 13.99740399 35.53913492616668 139.77784779322678 14.0124809 35.539136095928995 139.77784696314336 13.75268263 35.53912578952886 139.7778244951569 13.74958582 35.53912509011407 139.77782498163347 13.90416496</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_6">
											<gml:posList>35.539084722337016 139.77773834885195 13.99740399 35.53908239228874 139.7777400287081 13.99738757 35.53913258662366 139.77784945238557 14.01246165 35.53913492616668 139.77784779322678 14.0124809 35.539084722337016 139.77773834885195 13.99740399</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_7">
											<gml:posList>35.53911288717887 139.77779974721085 13.48624398 35.539110536133265 139.7778013812941 13.48622127 35.53908239226602 139.77774002769164 13.47777177 35.53908472231455 139.7777383478354 13.47778818 35.53911288717887 139.77779974721085 13.48624398</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2428_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2428_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2428_8">
											<gml:posList>35.539110536133265 139.7778013812941 13.48622127 35.53910936062074 139.77780219884144 13.74601782 35.53908122725315 139.77774086812795 13.73757147 35.53908239226602 139.77774002769164 13.47777177 35.539110536133265 139.7778013812941 13.48622127</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2426_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2426_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2426_0">
											<gml:posList>35.53911334546807 139.77779942950485 13.90457914 35.53912509011407 139.77782498163347 13.90416496 35.53917038232445 139.77779349841194 13.90464996 35.53915857523719 139.77776799274264 13.90506343 35.53911334546807 139.77779942950485 13.90457914</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2426_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2426_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2426_1">
											<gml:posList>35.53912178695962 139.77782727769875 13.90412979 35.53911007786231 139.77780170063895 13.90454435 35.539041045283334 139.77784968136078 13.90381587 35.539052861049065 139.77787518893743 13.90340237 35.53912178695962 139.77782727769875 13.90412979</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2423_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2423_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2423_0">
											<gml:posList>35.53908320545401 139.77772409106416 11.0658043 35.539083466684886 139.77772465498484 11.06579507 35.53916207508958 139.77766899037064 11.06665746 35.53916162440871 139.7776680071764 11.06667354 35.53908320545401 139.77772409106416 11.0658043</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2423_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2423_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2423_1">
											<gml:posList>35.53907216732157 139.77773265634139 11.06567242 35.539071934111554 139.7777321521239 11.06568067 35.5389926583487 139.77778884863673 11.06482047 35.53899269638008 139.7777889315917 11.06481912 35.53907216732157 139.77773265634139 11.06567242</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2422_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2422_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2422_0">
											<gml:posList>35.53915926382986 139.77766285535404 9.99175781 35.539985892619896 139.77707671482565 10.00180589 35.53997697247093 139.7770579245099 10.00211373 35.53821315236047 139.77831135247763 9.9827671 35.53822109256522 139.77832807749778 9.98249294 35.538989948621705 139.77778293616012 9.98991716 35.53899265828652 139.77778884654208 9.98982049 35.539071934062726 139.7777321500196 9.99068069 35.5390704262763 139.77772889007275 9.99073401 35.53908167327084 139.7777207815267 9.99085842 35.53908320540706 139.77772408895856 9.99080431 35.539161624375026 139.77766800506132 9.99167355 35.53915926382986 139.77766285535404 9.99175781</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2421_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_0">
											<gml:posList>35.53908853442274 139.77773559559245 11.52061624 35.53907727941839 139.7777437099254 11.52049175 35.53907483076655 139.77773841575527 11.52057826 35.538995342178154 139.7777947034961 11.51972477 35.539002059088496 139.77780935457938 11.51948554 35.53909801500271 139.7777440642617 11.52047509 35.53916743688085 139.7776968281189 11.52120555 35.539173076200875 139.77769299099972 11.52126542 35.53916472055649 139.77767476252157 11.52156309 35.53908613325895 139.77773041218376 11.52070094 35.53908853442274 139.77773559559245 11.52061624</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2421_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_1">
											<gml:posList>35.53909801500271 139.7777440642617 11.52047509 35.539002059088496 139.77780935457938 11.51948554 35.53900698757076 139.77782010470503 11.51931023 35.539017348367665 139.777812928377 11.51941798 35.53907296711235 139.77777440443546 11.52000109 35.53910253316926 139.77775392568384 11.52031428 35.53909801500271 139.7777440642617 11.52047509</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2421_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_2">
											<gml:posList>35.539017348367665 139.777812928377 11.51941798 35.53900698757076 139.77782010470503 11.51931023 35.53902329500807 139.77785567490926 11.51873157 35.53917308164505 139.77775277882841 11.52030227 35.53918371038936 139.7777454773849 11.52041589 35.53919392016655 139.7777384637499 11.52052531 35.539173076200875 139.77769299099972 11.52126542 35.53916743688085 139.7776968281189 11.52120555 35.53917856734687 139.7777211071054 11.52080995 35.53910914514835 139.77776835709943 11.52007925 35.53910253316926 139.77775392568384 11.52031428 35.53907296711235 139.77777440443546 11.52000109 35.53908170131841 139.77779324266487 11.51969431 35.53902607364918 139.77783177651878 11.51911104 35.539017348367665 139.777812928377 11.51941798</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2421_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_3">
											<gml:posList>35.53902549889007 139.7778604820765 11.51865354 35.539041045163486 139.77784967673625 11.51881591 35.53915857516131 139.77776798808756 11.52006346 35.53917486396553 139.7777566666121 11.52023915 35.53917308164505 139.77775277882841 11.52030227 35.53902329500807 139.77785567490926 11.51873157 35.53902549889007 139.7778604820765 11.51865354</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2421_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_4">
											<gml:posList>35.53919392016655 139.7777384637499 11.52052531 35.53918371038936 139.7777454773849 11.52041589 35.53919484340231 139.7777697619215 11.52002207 35.53918421467638 139.77777706336607 11.51990845 35.53917486396553 139.7777566666121 11.52023915 35.53915857516131 139.77776798808756 11.52006346 35.53917038225308 139.77779349376644 11.51965 35.53905286093364 139.7778751843224 11.5184024 35.539041045163486 139.77784967673625 11.51881591 35.53902549889007 139.7778604820765 11.51865354 35.539047955877194 139.7779094659063 11.51786057 35.53921726594385 139.77778939447148 11.51970053 35.53919392016655 139.7777384637499 11.52052531</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2421_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_9">
											<gml:posList>35.53907483087895 139.7777384206463 14.02057822 35.53907216745566 139.77773266212535 14.02067237 35.5389926965512 139.77778893734964 14.01981907 35.538995342321826 139.77779470836518 14.01972474 35.53907483087895 139.7777384206463 14.02057822</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2421_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_10">
											<gml:posList>35.53908346681376 139.7777246607725 14.02079503 35.539086133366844 139.777730417078 14.02070091 35.539164720633586 139.7776747674377 14.02156305 35.539162075181856 139.77766899618413 14.02165741 35.53908346681376 139.7777246607725 14.02079503</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2424_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2424_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2424_0">
											<gml:posList>35.53909801512288 139.7777440699524 14.43047504 35.53910914526346 139.77776836277903 14.4300792 35.53917856743027 139.77772111280652 14.4308099 35.539167436969294 139.77769683383102 14.4312055 35.53909801512288 139.7777440699524 14.43047504</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2425_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2425_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2425_0">
											<gml:posList>35.5390173484892 139.7778129327527 13.76941795 35.539026073767666 139.77783178088777 13.769111 35.539081701417146 139.77779324704753 13.76969427 35.53907296721415 139.7777744088247 13.77000106 35.5390173484892 139.7778129327527 13.76941795</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0275_p2427_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2427_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2427_0">
											<gml:posList>35.53917308171702 139.77775278359667 13.96030223 35.539184214744104 139.77777706812492 13.95990841 35.53919484346598 139.7777697666832 13.96002204 35.53918371045728 139.77774548215592 13.96041585 35.53917308171702 139.77775278359667 13.96030223</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2467_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2467_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2467_1">
											<gml:posList>35.54005009984591 139.7776954729875 17.52102682 35.54005622868957 139.77770862294545 17.58644764 35.5400582236093 139.77771290311875 17.56285561 35.54005822338508 139.77771290712496 19.60638129 35.54005009961965 139.77769547708192 19.60665158 35.54005009984591 139.7776954729875 17.52102682</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2467_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2467_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2467_2">
											<gml:posList>35.54005009984591 139.7776954729875 17.52102682 35.54005009961965 139.77769547708192 19.60665158 35.54006889387696 139.77768248729893 19.60688684 35.54006889410917 139.77768248320217 17.52224935 35.54005009984591 139.7776954729875 17.52102682</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2467_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2467_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2467_3">
											<gml:posList>35.54006889410917 139.77768248320217 17.52224935 35.54006889387696 139.77768248729893 19.60688684 35.540077008016254 139.77769989691328 19.60661686 35.54007700824669 139.777699892901 17.56211646 35.54007493069206 139.77769543537426 17.58668597 35.54006889410917 139.77768248320217 17.52224935</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2467_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2467_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2467_4">
											<gml:posList>35.54007700824669 139.777699892901 17.56211646 35.540077008016254 139.77769989691328 19.60661686 35.54005822338508 139.77771290712496 19.60638129 35.5400582236093 139.77771290311875 17.56285561 35.54007700824669 139.777699892901 17.56211646</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2373_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_6">
											<gml:posList>35.54005338144049 139.7772118730159 3.67761388 35.54005338012617 139.7772118976093 15.73961368 35.54038972660206 139.77792186999406 15.72859464 35.540389727690744 139.77792185702995 9.00359474 35.540389727933594 139.77792185413838 7.50359477 35.54038972855478 139.77792184674163 3.66659483 35.54005338144049 139.7772118730159 3.67761388</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2373_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_7">
											<gml:posList>35.53863326663344 139.77916520005996 7.48436602 35.54038088407035 139.77792811460156 7.503478 35.54038088382966 139.77792811749174 9.00347797 35.540387124976206 139.77792369946013 9.00356036 35.540389727690744 139.77792185702995 9.00359474 35.54038972660206 139.77792186999406 15.72859464 35.5386216306677 139.77917345151639 15.70926508 35.53862162926576 139.7791734306288 3.64726523 35.54038972855478 139.77792184674163 3.66659483 35.540389727933594 139.77792185413838 7.50359477 35.54038712521844 139.77792369656896 7.50356038 35.540387125242646 139.7779236962799 7.35356038 35.53862702543308 139.77916961763162 7.3343119 35.53862702562339 139.7791696204899 8.98431188 35.538633266805064 139.77916520265939 8.984366 35.53863326663344 139.77916520005996 7.48436602</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2373_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_8">
											<gml:posList>35.53828528715652 139.7784635003247 14.67029111 35.53830005738425 139.77849467682412 14.66978868 35.538300055547104 139.77849465658426 3.65778884 35.53862162926576 139.7791734306288 3.64726523 35.5386216306677 139.77917345151639 15.70926508 35.53828528733408 139.77846350225968 15.72029109 35.53828528715652 139.7784635003247 14.67029111</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2373_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_9">
											<gml:posList>35.53901784401095 139.77794494273454 14.61731755 35.539008108111005 139.77795183459122 14.61721509 35.539008108047796 139.77795183239894 13.4772151 35.53828897889179 139.77846088471628 13.47032306 35.53828897909408 139.77846088692814 14.67032304 35.53828528715652 139.7784635003247 14.67029111 35.53828528733408 139.77846350225968 15.72029109 35.54005338012617 139.7772118976093 15.73961368 35.54005338144049 139.7772118730159 3.67761388 35.54005187501377 139.77721293941875 3.67759398 35.54005187381654 139.77721296186945 14.68959379 35.5400456327716 139.77721737991865 14.68951142 35.5400456329009 139.77721737747302 13.48951144 35.539017843949445 139.7779449405411 13.47731757 35.53901784401095 139.77794494273454 14.61731755</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2373_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2373_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2373_10">
											<gml:posList>35.540387124976206 139.77792369946013 9.00356036 35.54038712521844 139.77792369656896 7.50356038 35.540389727933594 139.77792185413838 7.50359477 35.540389727690744 139.77792185702995 9.00359474 35.540387124976206 139.77792369946013 9.00356036</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2466_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2466_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2466_1">
											<gml:posList>35.540151548140784 139.77764639273877 17.56697446 35.540151547886914 139.77764639676056 19.60759092 35.54014342515652 139.77762897570813 19.60786109 35.54014342541368 139.77762897158686 17.51963144 35.54014979940239 139.777642642215 17.58764907 35.540151548140784 139.77764639273877 17.56697446</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2466_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2466_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2466_2">
											<gml:posList>35.54014342541368 139.77762897158686 17.51963144 35.54014342515652 139.77762897570813 19.60786109 35.540162226819135 139.7776159778678 19.608101 35.540162227082334 139.77761597374425 17.5208481 35.54014342541368 139.77762897158686 17.51963144</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2466_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2466_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2466_3">
											<gml:posList>35.540170342064485 139.77763339283965 17.56616974 35.54016850587175 139.77762945145815 17.58789197 35.540162227082334 139.77761597374425 17.5208481 35.540162226819135 139.7776159778678 19.608101 35.5401703418044 139.77763339686763 19.60783085 35.540170342064485 139.77763339283965 17.56616974</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2466_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2466_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2466_4">
											<gml:posList>35.540170342064485 139.77763339283965 17.56616974 35.5401703418044 139.77763339686763 19.60783085 35.540151547886914 139.77764639676056 19.60759092 35.540151548140784 139.77764639273877 17.56697446 35.540170342064485 139.77763339283965 17.56616974</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2465_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2465_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2465_1">
											<gml:posList>35.53940259105528 139.7781764831761 17.5497949 35.53940259104148 139.77817648710055 19.62857627 35.53939447569487 139.77815906728983 19.62884632 35.53939447570612 139.77815906329926 17.51805377 35.53940015130944 139.77817124624977 17.57865743 35.53940259105528 139.7781764831761 17.5497949</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2465_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2465_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2465_2">
											<gml:posList>35.53939447570612 139.77815906329926 17.51805377 35.53939447569487 139.77815906728983 19.62884632 35.53941326865358 139.77814607073523 19.62905007 35.53941326867113 139.77814606674212 17.51921497 35.53939447570612 139.77815906329926 17.51805377</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2465_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2465_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2465_3">
											<gml:posList>35.53942139265985 139.77816348384837 17.54893046 35.53941886003109 139.77815805419255 17.57886422 35.53941326867113 139.77814606674212 17.51921497 35.53941326865358 139.77814607073523 19.62905007 35.53942139263992 139.77816348777898 19.62878007 35.53942139265985 139.77816348384837 17.54893046</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2465_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2465_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2465_4">
											<gml:posList>35.53942139265985 139.77816348384837 17.54893046 35.53942139263992 139.77816348777898 19.62878007 35.53940259104148 139.77817648710055 19.62857627 35.53940259105528 139.7781764831761 17.5497949 35.53942139265985 139.77816348384837 17.54893046</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2464_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2464_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2464_1">
											<gml:posList>35.539496436333565 139.77811031311305 17.55082874 35.5394964362893 139.77811031704954 19.62462255 35.5394883211927 139.7780929089039 19.62489243 35.539488321235034 139.77809290490103 17.51915572 35.53949399435705 139.77810507474805 17.57970374 35.539496436333565 139.77811031311305 17.55082874</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2464_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2464_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2464_2">
											<gml:posList>35.539488321235034 139.77809290490103 17.51915572 35.5394883211927 139.7780929089039 19.62489243 35.53950711413216 139.7780799101479 19.62510073 35.53950711418071 139.7780799061426 17.52031329 35.539488321235034 139.77809290490103 17.51915572</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2464_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2464_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2464_3">
											<gml:posList>35.539515238107064 139.77809732554596 17.54991935 35.539512700504815 139.7780918844697 17.57991502 35.53950711418071 139.7780799061426 17.52031329 35.53950711413216 139.7780799101479 19.62510073 35.539515238056595 139.77809732948867 19.62483069 35.539515238107064 139.77809732554596 17.54991935</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2464_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2464_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2464_4">
											<gml:posList>35.539515238107064 139.77809732554596 17.54991935 35.539515238056595 139.77809732948867 19.62483069 35.5394964362893 139.77811031704954 19.62462255 35.539496436333565 139.77811031311305 17.55082874 35.539515238107064 139.77809732554596 17.54991935</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2463_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2463_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2463_1">
											<gml:posList>35.53958976549244 139.7780438169742 17.5547439 35.53958976541799 139.7780438209173 19.62069641 35.53958165021464 139.77802639252116 19.62096663 35.53958165028779 139.77802638850085 17.51757384 35.53958756638461 139.77803909417054 17.58076962 35.53958976549244 139.7780438169742 17.5547439</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2463_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2463_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2463_2">
											<gml:posList>35.53958165028779 139.77802638850085 17.51757384 35.53958165021464 139.77802639252116 19.62096663 35.53960044330579 139.7780134039267 19.62117928 35.53960044338506 139.77801339990407 17.51877447 35.53958165028779 139.77802638850085 17.51757384</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2463_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2463_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2463_3">
											<gml:posList>35.53960856737973 139.7780308292573 17.55383916 35.539606271806115 139.77802590436843 17.5809854 35.53960044338506 139.77801339990407 17.51877447 35.53960044330579 139.7780134039267 19.62117928 35.539608567299155 139.7780308332067 19.62090906 35.53960856737973 139.7780308292573 17.55383916</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2463_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2463_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2463_4">
											<gml:posList>35.53960856737973 139.7780308292573 17.55383916 35.539608567299155 139.7780308332067 19.62090906 35.53958976541799 139.7780438209173 19.62069641 35.53958976549244 139.7780438169742 17.5547439 35.53960856737973 139.7780308292573 17.55383916</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2462_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2462_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2462_1">
											<gml:posList>35.539309263627345 139.7782429773658 17.54592647 35.53930926364418 139.77824298128326 19.63254763 35.53930114802912 139.7782255618023 19.63281764 35.53930114800938 139.77822555782922 17.51960398 35.539306583056614 139.7782372238055 17.57763679 35.539309263627345 139.7782429773658 17.54592647</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2462_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2462_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2462_2">
											<gml:posList>35.53930114800938 139.77822555782922 17.51960398 35.53930114802912 139.7782255618023 19.63281764 35.53931994098064 139.77821257515572 19.63301673 35.53931994096711 139.77821257118015 17.52079758 35.53930114800938 139.77822555782922 17.51960398</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2462_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2462_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2462_3">
											<gml:posList>35.53932805591906 139.77822997763656 17.5450865 35.53932528585377 139.7782240359617 17.57783899 35.53931994096711 139.77821257118015 17.52079758 35.53931994098064 139.77821257515572 19.63301673 35.53932805592977 139.7782299815603 19.63274692 35.53932805591906 139.77822997763656 17.5450865</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2462_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2462_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2462_4">
											<gml:posList>35.53932805591906 139.77822997763656 17.5450865 35.53932805592977 139.7782299815603 19.63274692 35.53930926364418 139.77824298128326 19.63254763 35.539309263627345 139.7782429773658 17.54592647 35.53932805591906 139.77822997763656 17.5450865</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2461_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2461_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2461_1">
											<gml:posList>35.539776951885806 139.77791113806362 17.55979832 35.53977695175099 139.77791114203444 19.61790726 35.53976883646878 139.77789373526417 19.61817717 35.53976883660375 139.77789373120498 17.51728368 35.53977499188047 139.77790693402238 17.58297244 35.539776951885806 139.77791113806362 17.55979832</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2461_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2461_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2461_2">
											<gml:posList>35.53976883660375 139.77789373120498 17.51728368 35.53976883646878 139.77789373526417 19.61817717 35.53978763043254 139.777880733929 19.61839906 35.53978763057355 139.7778807298674 17.51844827 35.53976883660375 139.77789373120498 17.51728368</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2461_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2461_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2461_3">
											<gml:posList>35.53979574474562 139.77789815112635 17.5589254 35.53979369339117 139.77789374690596 17.5831972 35.53978763057355 139.7778807298674 17.51844827 35.53978763043254 139.777880733929 19.61839906 35.53979574460468 139.77789815510351 19.61812893 35.53979574474562 139.77789815112635 17.5589254</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2461_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2461_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2461_4">
											<gml:posList>35.53979574474562 139.77789815112635 17.5589254 35.53979574460468 139.77789815510351 19.61812893 35.53977695175099 139.77791114203444 19.61790726 35.539776951885806 139.77791113806362 17.55979832 35.53979574474562 139.77789815112635 17.5589254</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2460_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2460_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2460_1">
											<gml:posList>35.539683621300426 139.77797764580964 17.55579543 35.53968362119543 139.77797764977427 19.62178803 35.539675506057065 139.7779602315396 19.6220581 35.53967550616123 139.77796022749754 17.51874012 35.53968141783948 139.77797291637955 17.58186134 35.539683621300426 139.77797764580964 17.55579543</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2460_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2460_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2460_2">
											<gml:posList>35.53967550616123 139.77796022749754 17.51874012 35.539675506057065 139.7779602315396 19.6220581 35.53969429922999 139.77794723248405 19.62227544 35.53969429934036 139.77794722843964 17.5199065 35.53967550616123 139.77796022749754 17.51874012</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2460_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2460_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2460_3">
											<gml:posList>35.53970242292044 139.7779646483392 17.55493635 35.53970012657066 139.77795972420765 17.58208168 35.53969429934036 139.77794722843964 17.5199065 35.53969429922999 139.77794723248405 19.62227544 35.53970242280932 139.77796465231003 19.62200536 35.53970242292044 139.7779646483392 17.55493635</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2460_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2460_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2460_4">
											<gml:posList>35.53970242292044 139.7779646483392 17.55493635 35.53970242280932 139.77796465231003 19.62200536 35.53968362119543 139.77797764977427 19.62178803 35.539683621300426 139.77797764580964 17.55579543 35.53970242292044 139.7779646483392 17.55493635</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2459_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2459_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2459_1">
											<gml:posList>35.539870800079704 139.7778449879217 17.56082858 35.539870799914965 139.77784499190435 19.61404386 35.53986268483102 139.7778275733854 19.61431397 35.53986268499653 139.7778275693143 17.51847351 35.53986883208877 139.77784076380073 17.58410935 35.539870800079704 139.7778449879217 17.56082858</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2459_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2459_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2459_2">
											<gml:posList>35.53986268499653 139.7778275693143 17.51847351 35.53986268483102 139.7778275733854 19.61431397 35.539881469815974 139.7778145728909 19.61454036 35.53988146998762 139.77781456881735 17.5196221 35.53986268499653 139.7778275693143 17.51847351</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2459_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2459_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2459_3">
											<gml:posList>35.539889593220096 139.77783198923626 17.560008 35.539887535084944 139.77782757560075 17.58433866 35.53988146998762 139.77781456881735 17.5196221 35.539881469815974 139.7778145728909 19.61454036 35.539889593049125 139.77783199322522 19.61427023 35.539889593220096 139.77783198923626 17.560008</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2459_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2459_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2459_4">
											<gml:posList>35.539889593220096 139.77783198923626 17.560008 35.539889593049125 139.77783199322522 19.61427023 35.539870799914965 139.77784499190435 19.61404386 35.539870800079704 139.7778449879217 17.56082858 35.539889593220096 139.77783198923626 17.560008</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2458_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2458_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2458_1">
											<gml:posList>35.53996413234545 139.77777847825857 17.56487822 35.539964132151326 139.77777848224733 19.61020831 35.53995600838012 139.77776107413774 19.61047827 35.53995600857618 139.7777610700494 17.51703431 35.539962406769384 139.77777478063243 17.58526564 35.53996413234545 139.77777847825857 17.56487822</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2458_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2458_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2458_2">
											<gml:posList>35.53995600857618 139.7777610700494 17.51703431 35.53995600838012 139.77776107413774 19.61047827 35.53997480234707 139.77774806281255 19.61070935 35.53997480254926 139.77774805872178 17.51817084 35.53995600857618 139.7777610700494 17.51703431</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2458_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2458_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2458_3">
											<gml:posList>35.53998291690371 139.77776548998503 17.56404379 35.539981104312126 139.777761596242 17.58549939 35.53997480254926 139.77774805872178 17.51817084 35.53997480234707 139.77774806281255 19.61070935 35.53998291670345 139.7777654939801 19.61043902 35.53998291690371 139.77776548998503 17.56404379</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2458_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2458_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2458_4">
											<gml:posList>35.53998291690371 139.77776548998503 17.56404379 35.53998291670345 139.7777654939801 19.61043902 35.539964132151326 139.77777848224733 19.61020831 35.53996413234545 139.77777847825857 17.56487822 35.53998291690371 139.77776548998503 17.56404379</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2457_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2457_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2457_1">
											<gml:posList>35.53902946013057 139.77844126656865 17.53888539 35.53902946024023 139.7784412704566 19.64461505 35.53902134548771 139.77842384249914 19.64488514 35.53902134537433 139.77842383857075 17.52037209 35.539026432637264 139.77843476451832 17.57471579 35.53902946013057 139.77844126656865 17.53888539</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2457_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2457_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2457_2">
											<gml:posList>35.53902134537433 139.77842383857075 17.52037209 35.53902134548771 139.77842384249914 19.64488514 35.53904013748405 139.77841085511545 19.64507075 35.53904013737698 139.77841085118465 17.5215465 35.53902134537433 139.77842383857075 17.52037209</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2457_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2457_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2457_3">
											<gml:posList>35.53904825313942 139.7784282582451 17.53806596 35.53904513735018 139.77842157543137 17.57490451 35.53904013737698 139.77841085118465 17.5215465 35.53904013748405 139.77841085511545 19.64507075 35.539048253242875 139.7784282621391 19.64480097 35.53904825313942 139.7784282582451 17.53806596</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2457_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2457_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2457_4">
											<gml:posList>35.53904825313942 139.7784282582451 17.53806596 35.539048253242875 139.7784282621391 19.64480097 35.53902946024023 139.7784412704566 19.64461505 35.53902946013057 139.77844126656865 17.53888539 35.53904825313942 139.7784282582451 17.53806596</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2456_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2456_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2456_1">
											<gml:posList>35.53893612771237 139.77850776931686 17.53508311 35.53893612785348 139.77850777319728 19.64867638 35.538928021440015 139.7784903449759 19.64894645 35.53892802129537 139.7784903410653 17.52205448 35.53893286014696 139.77850074434429 17.57378521 35.53893612771237 139.77850776931686 17.53508311</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2456_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2456_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2456_2">
											<gml:posList>35.53892802129537 139.7784903410653 17.52205448 35.538928021440015 139.7784903449759 19.64894645 35.538946804278986 139.77847734669015 19.64912771 35.538946804140565 139.77847734277688 17.52315922 35.53892802129537 139.7784903410653 17.52205448</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2456_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2456_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2456_3">
											<gml:posList>35.538954928357406 139.77849477100654 17.53419545 35.538951564738284 139.77848755537894 17.57396942 35.538946804140565 139.77847734277688 17.52315922 35.538946804278986 139.77847734669015 19.64912771 35.5389549284924 139.77849477489323 19.64885764 35.538954928357406 139.77849477100654 17.53419545</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2456_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2456_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2456_4">
											<gml:posList>35.538954928357406 139.77849477100654 17.53419545 35.5389549284924 139.77849477489323 19.64885764 35.53893612785348 139.77850777319728 19.64867638 35.53893612771237 139.77850776931686 17.53508311 35.538954928357406 139.77849477100654 17.53419545</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2455_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2455_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2455_1">
											<gml:posList>35.53912330266919 139.7783750998756 17.53982586 35.53912330274767 139.778375103776 19.64057092 35.539115188414634 139.77835768739345 19.64084084 35.539115188332794 139.77835768345247 17.52138643 35.53912027247077 139.7783685960029 17.57567169 35.53912330266919 139.7783750998756 17.53982586</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2455_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2455_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2455_2">
											<gml:posList>35.539115188332794 139.77835768345247 17.52138643 35.539115188414634 139.77835768739345 19.64084084 35.539133980065365 139.7783446881949 19.64103115 35.539133979989835 139.77834468425124 17.52252039 35.539115188332794 139.77835768345247 17.52138643</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2455_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2455_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2455_3">
											<gml:posList>35.53914210415436 139.77836211278924 17.53889789 35.53913897796091 139.77835540633197 17.57586494 35.539133979989835 139.77834468425124 17.52252039 35.539133980065365 139.7783446881949 19.64103115 35.53914210422663 139.77836211669614 19.64076103 35.53914210415436 139.77836211278924 17.53889789</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2455_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2455_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2455_4">
											<gml:posList>35.53914210415436 139.77836211278924 17.53889789 35.53914210422663 139.77836211669614 19.64076103 35.53912330274767 139.778375103776 19.64057092 35.53912330266919 139.7783750998756 17.53982586 35.53914210415436 139.77836211278924 17.53889789</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2454_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2454_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2454_1">
											<gml:posList>35.539216628761004 139.7783086073514 17.54364784 35.53921662880848 139.7783086112592 19.63655462 35.53920852246616 139.77829118256787 19.63682476 35.5392085224155 139.77829117860932 17.51976829 35.53921384271798 139.7783026173988 17.57664743 35.539216628761004 139.7783086073514 17.54364784</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2454_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2454_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2454_2">
											<gml:posList>35.5392085224155 139.77829117860932 17.51976829 35.53920852246616 139.77829118256787 19.63682476 35.53922731480195 139.77827819479867 19.63701939 35.53922731475759 139.7782781908376 17.52095146 35.5392085224155 139.77829117860932 17.51976829</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2454_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2454_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2454_3">
											<gml:posList>35.539235430250905 139.77829559837522 17.54281511 35.539232552121554 139.77828942493227 17.57684524 35.53922731475759 139.7782781908376 17.52095146 35.53922731480195 139.77827819479867 19.63701939 35.539235430292166 139.77829560228912 19.63674958 35.539235430250905 139.77829559837522 17.54281511</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2454_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2454_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2454_4">
											<gml:posList>35.539235430250905 139.77829559837522 17.54281511 35.539235430292166 139.77829560228912 19.63674958 35.53921662880848 139.7783086112592 19.63655462 35.539216628761004 139.7783086073514 17.54364784 35.539235430250905 139.77829559837522 17.54281511</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2453_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2453_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2453_1">
											<gml:posList>35.538748947589376 139.77864043548576 17.53045653 35.538748947794026 139.77864043934545 19.6568724 35.53874082258712 139.77862302200145 19.65714229 35.538740822379125 139.77862301812144 17.52280409 35.53874543328232 139.77863290221745 17.5719891 35.538748947589376 139.77864043548576 17.53045653</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2453_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2453_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2453_2">
											<gml:posList>35.538740822379125 139.77862301812144 17.52280409 35.53874082258712 139.77862302200145 19.65714229 35.53875961408465 139.7786100133712 19.65731467 35.53875961388296 139.77861000948855 17.52388376 35.538740822379125 139.77862301812144 17.52280409</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2453_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2453_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2453_3">
											<gml:posList>35.53876773814014 139.77862743740755 17.52958863 35.5387641375337 139.77861971356376 17.57216428 35.53875961388296 139.77861000948855 17.52388376 35.53875961408465 139.7786100133712 19.65731467 35.53876773833857 139.77862744127358 19.65704463 35.53876773814014 139.77862743740755 17.52958863</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2453_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2453_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2453_4">
											<gml:posList>35.53876773814014 139.77862743740755 17.52958863 35.53876773833857 139.77862744127358 19.65704463 35.538748947794026 139.77864043934545 19.6568724 35.538748947589376 139.77864043548576 17.53045653 35.53876773814014 139.77862743740755 17.52958863</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2452_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2452_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2452_1">
											<gml:posList>35.538842278284214 139.77857393467974 17.53421209 35.5388422784566 139.77857393853816 19.64776586 35.53883415421932 139.77855651052076 19.64803592 35.53883415404323 139.77855650663201 17.52106158 35.5388390086363 139.77856692073232 17.57287451 35.538842278284214 139.77857393467974 17.53421209</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2452_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2452_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2452_2">
											<gml:posList>35.53883415404323 139.77855650663201 17.52106158 35.53883415421932 139.77855651052076 19.64803592 35.538852945905596 139.77854352350084 19.64821249 35.538852945735805 139.77854351960954 17.52222723 35.53883415404323 139.77855650663201 17.52106158</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2452_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2452_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2452_3">
											<gml:posList>35.53886106912411 139.77856093632695 17.53334884 35.5388577098993 139.77855373414988 17.57305417 35.538852945735805 139.77854351960954 17.52222723 35.538852945905596 139.77854352350084 19.64821249 35.538861069290284 139.77856094019154 19.6479426 35.53886106912411 139.77856093632695 17.53334884</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2452_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2452_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2452_4">
											<gml:posList>35.53886106912411 139.77856093632695 17.53334884 35.538861069290284 139.77856094019154 19.6479426 35.5388422784566 139.77857393853816 19.64776586 35.538842278284214 139.77857393467974 17.53421209 35.53886106912411 139.77856093632695 17.53334884</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2451_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2451_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2451_1">
											<gml:posList>35.53865509946053 139.7787065992111 17.52963233 35.53865509969699 139.77870660305777 19.66100722 35.53864698334572 139.77868917521593 19.66127724 35.53864698310565 139.77868917134896 17.52188577 35.53865159310609 139.77869907025246 17.57112384 35.53865509946053 139.7787065992111 17.52963233</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2451_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2451_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2451_2">
											<gml:posList>35.538665783288806 139.7786761735367 17.52302424 35.53864698310565 139.77868917134896 17.52188577 35.53864698334572 139.77868917521593 19.66127724 35.53866578352249 139.7786761774063 19.66144493 35.538665783288806 139.7786761735367 17.52302424</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2451_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2451_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2451_3">
											<gml:posList>35.538673890818345 139.77869359077565 17.52880071 35.53867030105389 139.77868587902842 17.57129452 35.538665783288806 139.7786761735367 17.52302424 35.53866578352249 139.7786761774063 19.66144493 35.53867389104869 139.77869359462852 19.66117508 35.538673890818345 139.77869359077565 17.52880071</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2451_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2451_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2451_4">
											<gml:posList>35.538673890818345 139.77869359077565 17.52880071 35.53867389104869 139.77869359462852 19.66117508 35.53865509969699 139.77870660305777 19.66100722 35.53865509946053 139.7787065992111 17.52963233 35.538673890818345 139.77869359077565 17.52880071</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2450_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2450_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2450_1">
											<gml:posList>35.53856176894591 139.77877308720628 17.52597313 35.538561769214105 139.77877309103584 19.66015914 35.5385536527924 139.77875568518243 19.66042881 35.538553652521244 139.77875568134237 17.52367209 35.53855802077939 139.77876504924296 17.57028364 35.53856176894591 139.77877308720628 17.52597313</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2450_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2450_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2450_2">
											<gml:posList>35.538553652521244 139.77875568134237 17.52367209 35.5385536527924 139.77875568518243 19.66042881 35.538572446206324 139.77874267693204 19.66059215 35.53857244594147 139.77874267308934 17.52474895 35.538553652521244 139.77875568134237 17.52367209</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2450_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2450_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2450_3">
											<gml:posList>35.538580560253976 139.77876010069434 17.52504626 35.53857672399374 139.77875186139238 17.57044977 35.53857244594147 139.77874267308934 17.52474895 35.538572446206324 139.77874267693204 19.66059215 35.53858056051595 139.7787601045302 19.66032217 35.538580560253976 139.77876010069434 17.52504626</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2450_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2450_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2450_4">
											<gml:posList>35.538580560253976 139.77876010069434 17.52504626 35.53858056051595 139.7787601045302 19.66032217 35.538561769214105 139.77877309103584 19.66015914 35.53856176894591 139.77877308720628 17.52597313 35.538580560253976 139.77876010069434 17.52504626</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2375_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_1">
											<gml:posList>35.54038088382966 139.77792811749174 9.00347797 35.54038088407035 139.77792811460156 7.503478 35.54041811254345 139.7780059255325 7.50232389 35.54041811229392 139.77800592840438 9.00232386 35.54038088382966 139.77792811749174 9.00347797</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2375_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_2">
											<gml:posList>35.53867314833553 139.77924863634433 8.98312677 35.54041811229392 139.77800592840438 9.00232386 35.54041811254345 139.7780059255325 7.50232389 35.53867314817338 139.7792486337644 7.48312679 35.53867314833553 139.77924863634433 8.98312677</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2375_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_3">
											<gml:posList>35.53863326663344 139.77916520005996 7.48436602 35.538633266805064 139.77916520265939 8.984366 35.53867314833553 139.77924863634433 8.98312677 35.53867314817338 139.7792486337644 7.48312679 35.53863326663344 139.77916520005996 7.48436602</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2375_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_5">
											<gml:posList>35.540387125242646 139.7779236962799 7.35356038 35.54038712521844 139.77792369656896 7.50356038 35.540387124976206 139.77792369946013 9.00356036 35.54042798646432 139.77800910373406 9.00229419 35.5404279867414 139.7780091005758 7.35229422 35.540387125242646 139.7779236962799 7.35356038</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2375_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_6">
											<gml:posList>35.5404279867414 139.7780091005758 7.35229422 35.54042798646432 139.77800910373406 9.00229419 35.538670556729706 139.77926068923915 8.98295984 35.538670556550585 139.77926068640437 7.33295986 35.5404279867414 139.7780091005758 7.35229422</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2375_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2375_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2375_7">
											<gml:posList>35.53862702562339 139.7791696204899 8.98431188 35.53862702543308 139.77916961763162 7.3343119 35.538670556550585 139.77926068640437 7.33295986 35.538670556729706 139.77926068923915 8.98295984 35.53862702562339 139.7791696204899 8.98431188</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_1">
											<gml:posList>35.5392050053586 139.77770963136933 13.48098631 35.53921098777106 139.77772256844077 13.48077602 35.539210987794114 139.77772257036057 14.460776 35.53920500538257 139.77770963329098 14.4609863 35.5392050053586 139.77770963136933 13.48098631</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_2">
											<gml:posList>35.53926274621266 139.7776696866662 14.46161734 35.53926274619752 139.77766968473838 13.48161736 35.5392050053586 139.77770963136933 13.48098631 35.53920500538257 139.77770963329098 14.4609863 35.53926274621266 139.7776696866662 14.46161734</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_3">
											<gml:posList>35.53927440439789 139.77769417835947 13.48121941 35.53926274619752 139.77766968473838 13.48161736 35.53926274621266 139.7776696866662 14.46161734 35.53927440441112 139.77769418028356 14.4612194 35.53927440439789 139.77769417835947 13.48121941</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_4">
											<gml:posList>35.53965180140265 139.77742456519388 14.46569437 35.53965180144753 139.77742456322838 13.48569439 35.53927440439789 139.77769417835947 13.48121941 35.53927440441112 139.77769418028356 14.4612194 35.53965180140265 139.77742456519388 14.46569437</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_5">
											<gml:posList>35.53965718554194 139.77743774111565 13.48548021 35.53965180144753 139.77742456322838 13.48569439 35.53965180140265 139.77742456519388 14.46569437 35.53965718549625 139.77743774307908 14.4654802 35.53965718554194 139.77743774111565 13.48548021</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_6">
											<gml:posList>35.539278566481855 139.77770821617406 14.46099222 35.539278566469164 139.77770821425207 13.48099223 35.53953791338788 139.77752294558144 13.4840263 35.53965718554194 139.77743774111565 13.48548021 35.53965718549625 139.77743774307908 14.4654802 35.539278566481855 139.77770821617406 14.46099222</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_7">
											<gml:posList>35.539292550696054 139.77773762393684 14.46051644 35.53929255068552 139.77773762201946 13.48051646 35.5392787182515 139.77770853343898 13.48098706 35.539278566469164 139.77770821425207 13.48099223 35.539278566481855 139.77770821617406 14.46099222 35.539292550696054 139.77773762393684 14.46051644</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_8">
											<gml:posList>35.53923044527465 139.77777987160098 13.47984972 35.53929255068552 139.77773762201946 13.48051646 35.539292550696054 139.77773762393684 14.46051644 35.53923044529472 139.77777987351197 14.4598497 35.53923044527465 139.77777987160098 13.47984972</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_9">
											<gml:posList>35.53922372665938 139.7777654957545 14.46008214 35.53922372663831 139.77776549384131 13.48008215 35.53923044527465 139.77777987160098 13.47984972 35.53923044529472 139.77777987351197 14.4598497 35.53922372665938 139.7777654957545 14.46008214</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_10">
											<gml:posList>35.53927532936196 139.77772980198682 14.4606449 35.53927532934882 139.77772980006824 13.48064491 35.53922372663831 139.77776549384131 13.48008215 35.53922372665938 139.7777654957545 14.46008214 35.53927532936196 139.77772980198682 14.4606449</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_11">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_11">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_11">
											<gml:posList>35.53925721112698 139.77769059787366 13.48128039 35.53926875993461 139.7777155858948 13.48087503 35.53927532934882 139.77772980006824 13.48064491 35.53927532936196 139.77772980198682 14.4606449 35.539257211142925 139.7776905997983 14.46128038 35.53925721112698 139.77769059787366 13.48128039</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2431_12">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2431_12">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2431_12">
											<gml:posList>35.539210987794114 139.77772257036057 14.460776 35.53921098777106 139.77772256844077 13.48077602 35.53925721112698 139.77769059787366 13.48128039 35.539257211142925 139.7776905997983 14.46128038 35.539210987794114 139.77772257036057 14.460776</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_17">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_17">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_17">
											<gml:posList>35.53828897909408 139.77846088692814 14.67032304 35.53828897889179 139.77846088471628 13.47032306 35.53824675545464 139.77837118423795 13.47177737 35.53822872105091 139.77833287159189 13.47240274 35.538228721264566 139.7783328738279 14.67240272 35.53828897909408 139.77846088692814 14.67032304</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_18">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_18">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_18">
											<gml:posList>35.53822872105091 139.77833287159189 13.47240274 35.538987237436544 139.77779506147044 13.47972487 35.53898723750702 139.7777950638075 14.67972485 35.538228721264566 139.7783328738279 14.67240272 35.53822872105091 139.77833287159189 13.47240274</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_19">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_19">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_19">
											<gml:posList>35.539045244945875 139.77792159119787 14.67766995 35.53898723750702 139.7777950638075 14.67972485 35.538987237436544 139.77779506147044 13.47972487 35.53899455538887 139.77781102355937 13.47946414 35.53899455545452 139.77781102577677 14.61946412 35.53899959644961 139.7778220213178 14.61928476 35.538999596384954 139.7778220191024 13.47928478 35.539000461194334 139.7778239054461 13.47925403 35.53902864727748 139.77788538568424 13.47825516 35.539038735456444 139.777907390317 13.47789921 35.53904524488631 139.77792158888465 13.47766997 35.539045244945875 139.77792159119787 14.67766995</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_20">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_20">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_20">
											<gml:posList>35.53904524488631 139.77792158888465 13.47766997 35.539227032088164 139.77779266892338 13.47964548 35.5392270321134 139.7777926712609 14.67964546 35.539045244945875 139.77792159119787 14.67766995 35.53904524488631 139.77792158888465 13.47766997</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_21">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_21">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_21">
											<gml:posList>35.53917871241535 139.7776872556707 13.48135607 35.53916903029764 139.77766613339162 13.48170109 35.5391690303338 139.77766613575284 14.68170108 35.5392270321134 139.7777926712609 14.67964546 35.539227032088164 139.77779266892338 13.47964548 35.53921058394814 139.77775678594608 13.48022565 35.53917871241535 139.7776872556707 13.48135607</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_22">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_22">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_22">
											<gml:posList>35.539984710380736 139.7770877563647 13.49161754 35.539984710263 139.77708775883468 14.69161752 35.5391690303338 139.77766613575284 14.68170108 35.53916903029764 139.77766613339162 13.48170109 35.539984710380736 139.7770877563647 13.49161754</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_23">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_23">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_23">
											<gml:posList>35.5400456329009 139.77721737747302 13.48951144 35.5400456327716 139.77721737991865 14.68951142 35.539984710263 139.77708775883468 14.69161752 35.539984710380736 139.7770877563647 13.49161754 35.53998954297799 139.77709803836734 13.49144942 35.53999065024457 139.77710039422678 13.49141093 35.539991183117266 139.7771015279855 13.49139241 35.53999304192158 139.77710548284375 13.49132781 35.53999870973325 139.77711754188448 13.49113103 35.54002656917026 139.77717681668992 13.49016738 35.5400456329009 139.77721737747302 13.48951144</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_25">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_25">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_25">
											<gml:posList>35.53898994825303 139.7777829238357 3.66791726 35.538218882331726 139.77832963201197 3.6604739 35.538218884309465 139.77832965253637 14.67247374 35.538989948895164 139.77778294530313 14.67991709 35.538989948621705 139.77778293616012 9.98991716 35.53822109256522 139.77832807749778 9.98249294 35.53822109253841 139.77832807721822 9.83249294 35.538989948612915 139.7777829358678 9.83991716 35.53898994825303 139.7777829238357 3.66791726</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_26">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_26">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_26">
											<gml:posList>35.53829743229205 139.77849650539054 3.65776629 35.538297434133696 139.77849652562722 14.66976614 35.538218884309465 139.77832965253637 14.67247374 35.538218882331726 139.77832963201197 3.6604739 35.53829743229205 139.77849650539054 3.65776629</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_27">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_27">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_27">
											<gml:posList>35.53899265827783 139.77778884624985 9.83982049 35.53899265792056 139.7777888342235 3.66782059 35.53898994825303 139.7777829238357 3.66791726 35.538989948612915 139.7777829358678 9.83991716 35.53899265827783 139.77778884624985 9.83982049</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_28">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_28">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_28">
											<gml:posList>35.5391616241771 139.777667992623 3.66967366 35.53916162437037 139.7776680047662 9.84167356 35.53915926382513 139.77766285505885 9.84175781 35.5391592636296 139.7776628429107 3.66975791 35.5391616241771 139.777667992623 3.66967366</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_29">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_29">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_29">
											<gml:posList>35.539987355398786 139.7770756650125 3.67982534 35.5391592636296 139.7776628429107 3.66975791 35.53915926382513 139.77766285505885 9.84175781 35.53998589263471 139.77707671451665 9.85180589 35.539985892619896 139.77707671482565 10.00180589 35.53915926382986 139.77766285535404 9.99175781 35.539159263978426 139.77766286458527 14.68175773 35.53998735431328 139.7770756876999 14.69182514 35.539987355398786 139.7770756650125 3.67982534</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_30">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_30">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_30">
											<gml:posList>35.54005187381654 139.77721296186945 14.68959379 35.54005187501377 139.77721293941875 3.67759398 35.539987355398786 139.7770756650125 3.67982534 35.53998735431328 139.7770756876999 14.69182514 35.54005187381654 139.77721296186945 14.68959379</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_31">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_31">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_31">
											<gml:posList>35.538300055547104 139.77849465658426 3.65778884 35.53830005738425 139.77849467682412 14.66978868 35.538297434133696 139.77849652562722 14.66976614 35.53829743229205 139.77849650539054 3.65776629 35.538300055547104 139.77849465658426 3.65778884</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_32">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_32">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_32">
											<gml:posList>35.5389926583487 139.77778884863673 11.06482047 35.53899265828652 139.77778884654208 9.98982049 35.538989948621705 139.77778293616012 9.98991716 35.538989948895164 139.77778294530313 14.67991709 35.539047956032704 139.77790947200396 14.67786052 35.53904795597367 139.77790946968838 13.47786054 35.539047955877194 139.7779094659063 11.51786057 35.53902549889007 139.7778604820765 11.51865354 35.53902329500807 139.77785567490926 11.51873157 35.53900698757076 139.77782010470503 11.51931023 35.539002059088496 139.77780935457938 11.51948554 35.538995342178154 139.7777947034961 11.51972477 35.538995342321826 139.77779470836518 14.01972474 35.5389926965512 139.77778893734964 14.01981907 35.53899269638008 139.7777889315917 11.06481912 35.5389926583487 139.77778884863673 11.06482047</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_33">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_33">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_33">
											<gml:posList>35.53904795597367 139.77790946968838 13.47786054 35.539217265987965 139.77778939829045 13.4797005 35.53921726594385 139.77778939447148 11.51970053 35.539047955877194 139.7779094659063 11.51786057 35.53904795597367 139.77790946968838 13.47786054</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_34">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_34">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_34">
											<gml:posList>35.539217265988334 139.77778939831748 13.49355177 35.53921726601509 139.77778940062865 14.67970048 35.539159263978426 139.77766286458527 14.68175773 35.53915926382986 139.77766285535404 9.99175781 35.539161624375026 139.77766800506132 9.99167355 35.53916162440871 139.7776680071764 11.06667354 35.53916207508958 139.77766899037064 11.06665746 35.539162075181856 139.77766899618413 14.02165741 35.539164720633586 139.7776747674377 14.02156305 35.53916472055649 139.77767476252157 11.52156309 35.539173076200875 139.77769299099972 11.52126542 35.53919392016655 139.7777384637499 11.52052531 35.53921726594385 139.77778939447148 11.51970053 35.539217265987965 139.77778939829045 13.4797005 35.539217265988334 139.77778939831748 13.49355177</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_35">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_35">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_35">
											<gml:posList>35.53913726569138 139.77784613305994 13.49288437 35.539136095928995 139.77784696314336 13.75268263 35.53913492616668 139.77784779322678 14.0124809 35.53913375640447 139.77784862330995 14.27227918 35.53913558670581 139.77784732609396 14.67880359 35.53921726601509 139.77778940062865 14.67970048 35.539217265988334 139.77778939831748 13.49355177 35.53913726569138 139.77784613305994 13.49288437</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2376_36">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2376_36">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2376_36">
											<gml:posList>35.5391349261482 139.77784779221903 13.4928651 35.539136095928995 139.77784696314336 13.75268263 35.53913726569138 139.77784613305994 13.49288437 35.539217265988334 139.77778939831748 13.49355177 35.539217265987965 139.77778939829045 13.4797005 35.53904795597367 139.77790946968838 13.47786054 35.539047956032704 139.77790947200396 14.67786052 35.53913558670581 139.77784732609396 14.67880359 35.53913375640447 139.77784862330995 14.27227918 35.53913492616668 139.77784779322678 14.0124809 35.53913258662366 139.77784945238557 14.01246165 35.53913141684275 139.77785028146127 13.75264412 35.539132586604985 139.77784945137805 13.49284584 35.5391349261482 139.77784779221903 13.4928651</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_1">
											<gml:posList>35.53898739731333 139.7778164165857 13.47938301 35.538980419628466 139.7778171318906 13.47937666 35.53898041969665 139.7778171341069 14.61937665 35.53898739738033 139.7778164188021 14.61938299 35.53898739731333 139.7778164165857 13.47938301</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_2">
											<gml:posList>35.53899455545452 139.77781102577677 14.61946412 35.53899455538887 139.77781102355937 13.47946414 35.53898739731333 139.7778164165857 13.47938301 35.53898739738033 139.7778164188021 14.61938299 35.53899455545452 139.77781102577677 14.61946412</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_3">
											<gml:posList>35.538999596384954 139.7778220191024 13.47928478 35.53899959644961 139.7778220213178 14.61928476 35.53899233046144 139.77782748349665 14.61920262 35.53899233039534 139.7778274812822 13.47920263 35.538999596384954 139.7778220191024 13.47928478</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_4">
											<gml:posList>35.53898536166496 139.77782818664184 13.47919638 35.53899233039534 139.7778274812822 13.47920263 35.53899233046144 139.77782748349665 14.61920262 35.53898536173232 139.77782818885615 14.61919637 35.53898536166496 139.77782818664184 13.47919638</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_5">
											<gml:posList>35.53896629662739 139.77784099107444 13.47900616 35.53898536166496 139.77782818664184 13.47919638 35.53898536173232 139.77782818885615 14.61919637 35.53896629669809 139.77784099328645 14.61900614 35.53896629662739 139.77784099107444 13.47900616</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_6">
											<gml:posList>35.53900279223815 139.77791999973437 14.61772285 35.53900279217395 139.7779199975365 13.47772287 35.538997116972034 139.77790771170686 13.47792173 35.53896901656117 139.77784687924787 13.47891015 35.53896629662739 139.77784099107444 13.47900616 35.53896629669809 139.77784099328645 14.61900614 35.53900279223815 139.77791999973437 14.61772285</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_7">
											<gml:posList>35.53900862228329 139.77792545507643 13.47763235 35.53900279217395 139.7779199975365 13.47772287 35.53900279223815 139.77791999973437 14.61772285 35.5390086223464 139.77792545727343 14.61763233 35.53900862228329 139.77792545507643 13.47763235</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_8">
											<gml:posList>35.53901784401095 139.77794494273454 14.61731755 35.539017843949445 139.7779449405411 13.47731757 35.539009783754395 139.77792790927498 13.47759266 35.53900862228329 139.77792545507643 13.47763235 35.5390086223464 139.77792545727343 14.61763233 35.53901784401095 139.77794494273454 14.61731755</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_9">
											<gml:posList>35.539008108047796 139.77795183239894 13.4772151 35.539008108111005 139.77795183459122 14.61721509 35.53895454884419 139.77783467157377 14.61911615 35.538954548771336 139.77783466936054 13.47911617 35.53896127981299 139.7778493937584 13.47887598 35.538962355166845 139.77785174613462 13.47883764 35.538963893537876 139.77785511137816 13.47878281 35.53899018331734 139.777912621296 13.47784875 35.53900025813189 139.77793466037164 13.47749228 35.539001481695834 139.77793733696947 13.47744904 35.539008108047796 139.77795183239894 13.4772151</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2430_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2430_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2430_10">
											<gml:posList>35.538980419628466 139.7778171318906 13.47937666 35.538954548771336 139.77783466936054 13.47911617 35.53895454884419 139.77783467157377 14.61911615 35.53898041969665 139.7778171341069 14.61937665 35.538980419628466 139.7778171318906 13.47937666</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2409_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2409_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2409_1">
											<gml:posList>35.53876079675686 139.7780750450012 13.47550436 35.538760797016785 139.7780750502459 16.23050431 35.53877008113475 139.7780977986141 16.2301378 35.53877008087879 139.77809779337932 13.47513784 35.53876079675686 139.7780750450012 13.47550436</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2409_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2409_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2409_2">
											<gml:posList>35.53877008087879 139.77809779337932 13.47513784 35.53877008113475 139.7780977986141 16.2301378 35.538740720597715 139.77811860716298 16.22984732 35.53874072032905 139.7781186019371 13.47484736 35.53877008087879 139.77809779337932 13.47513784</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2409_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2409_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2409_3">
											<gml:posList>35.53874072032905 139.7781186019371 13.47484736 35.538740720597715 139.77811860716298 16.22984732 35.5387314427208 139.77809583531513 16.23021411 35.53873144244816 139.77809583007948 13.47521415 35.5387325493358 139.7780985468507 13.47517035 35.53874072032905 139.7781186019371 13.47484736</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2409_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2409_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2409_4">
											<gml:posList>35.53873144244816 139.77809583007948 13.47521415 35.5387314427208 139.77809583531513 16.23021411 35.538760797016785 139.7780750502459 16.23050431 35.53876079675686 139.7780750450012 13.47550436 35.53874024955288 139.77808959398268 13.47530099 35.53873144244816 139.77809583007948 13.47521415</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2408_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2408_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2408_1">
											<gml:posList>35.53894983411897 139.7779427118261 13.47740246 35.538949834296716 139.77794271711832 16.22740242 35.53895911977568 139.7779654776923 16.22703523 35.5389591196021 139.77796547240993 13.47703528 35.53895859555891 139.7779641878745 13.47705598 35.53894983411897 139.7779427118261 13.47740246</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2408_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2408_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2408_2">
											<gml:posList>35.5389591196021 139.77796547240993 13.47703528 35.53895911977568 139.7779654776923 16.22703523 35.53892975707815 139.77798627514085 16.22673064 35.53892975689176 139.7779862698675 13.47673068 35.5389591196021 139.77796547240993 13.47703528</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2408_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2408_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2408_3">
											<gml:posList>35.53892975689176 139.7779862698675 13.47673068 35.53892975707815 139.77798627514085 16.22673064 35.53892047461423 139.7779635162733 16.22709771 35.53892047442387 139.77796351099005 13.47709775 35.538929236982945 139.77798499514776 13.47675122 35.53892975689176 139.7779862698675 13.47673068</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2408_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2408_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2408_4">
											<gml:posList>35.53892047442387 139.77796351099005 13.47709775 35.53892047461423 139.7779635162733 16.22709771 35.538949834296716 139.77794271711832 16.22740242 35.53894983411897 139.7779427118261 13.47740246 35.53892047442387 139.77796351099005 13.47709775</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2407_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2407_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2407_1">
											<gml:posList>35.538927075156856 139.77792741269815 13.47766335 35.538911919659675 139.7778931375205 13.47822018 35.53891191985339 139.77789314282438 16.22322014 35.53892707534407 139.77792741798737 16.22266331 35.538927075156856 139.77792741269815 13.47766335</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2407_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2407_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2407_2">
											<gml:posList>35.538918265017195 139.7779338639008 16.22256866 35.53891826482619 139.77793385861435 13.4775687 35.538927075156856 139.77792741269815 13.47766335 35.53892707534407 139.77792741798737 16.22266331 35.538918265017195 139.7779338639008 16.22256866</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2407_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2407_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2407_3">
											<gml:posList>35.53890310953277 139.77789958879725 16.22312546 35.53890310933527 139.77789958349612 13.47812551 35.53891826482619 139.77793385861435 13.4775687 35.538918265017195 139.7779338639008 16.22256866 35.53890310953277 139.77789958879725 16.22312546</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2407_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2407_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2407_4">
											<gml:posList>35.53891191985339 139.77789314282438 16.22322014 35.538911919659675 139.7778931375205 13.47822018 35.53890310933527 139.77789958349612 13.47812551 35.53890310953277 139.77789958879725 16.22312546 35.53891191985339 139.77789314282438 16.22322014</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2406_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2406_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2406_1">
											<gml:posList>35.53873895542833 139.77806031114858 13.475761 35.538725382183046 139.77802961522806 13.47625928 35.53872379978932 139.77802603664344 13.47631747 35.538723800064766 139.77802604189972 16.22631743 35.538738955697184 139.7780603163902 16.22576096 35.53873895542833 139.77806031114858 13.475761</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2406_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2406_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2406_2">
											<gml:posList>35.538730147121484 139.77806675194978 16.22567079 35.538730146848756 139.77806674671092 13.47567083 35.53873895542833 139.77806031114858 13.475761 35.538738955697184 139.7780603163902 16.22576096 35.538730147121484 139.77806675194978 16.22567079</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2406_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2406_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2406_3">
											<gml:posList>35.53871499148476 139.7780324774969 16.22622723 35.538714991205524 139.7780324722434 13.47622728 35.538730146848756 139.77806674671092 13.47567083 35.538730147121484 139.77806675194978 16.22567079 35.53871499148476 139.7780324774969 16.22622723</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2406_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2406_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2406_4">
											<gml:posList>35.538723800064766 139.77802604189972 16.22631743 35.53872379978932 139.77802603664344 13.47631747 35.538714991205524 139.7780324722434 13.47622728 35.53871499148476 139.7780324774969 16.22622723 35.538723800064766 139.77802604189972 16.22631743</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2405_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2405_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2405_1">
											<gml:posList>35.53853833207631 139.7781582580344 13.47451235 35.538538332432665 139.77815826324323 16.2295123 35.538553488159316 139.77819253700383 16.22895623 35.53855348780946 139.77819253180982 13.47395627 35.53853833207631 139.7781582580344 13.47451235</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2405_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2405_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2405_2">
											<gml:posList>35.53855348780946 139.77819253180982 13.47395627 35.538553488159316 139.77819253700383 16.22895623 35.53854467334963 139.77819897413 16.22887031 35.53854467299607 139.77819896893874 13.47387035 35.53855348780946 139.77819253180982 13.47395627</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2405_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2405_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2405_3">
											<gml:posList>35.53854467299607 139.77819896893874 13.47387035 35.53854467334963 139.77819897413 16.22887031 35.53852951760415 139.77816470037024 16.22942635 35.538529517243994 139.7781646951642 13.47442639 35.53854467299607 139.77819896893874 13.47387035</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2405_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2405_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2405_4">
											<gml:posList>35.538529517243994 139.7781646951642 13.47442639 35.53852951760415 139.77816470037024 16.22942635 35.538538332432665 139.77815826324323 16.2295123 35.53853833207631 139.7781582580344 13.47451235 35.538529517243994 139.7781646951642 13.47442639</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2404_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2404_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2404_1">
											<gml:posList>35.5383672990796 139.77832457490305 13.47224478 35.53835321013021 139.77829344505537 13.47275006 35.538352626200414 139.77829215485008 13.47277104 35.53835262660788 139.7782921596552 16.042771 35.53836729948119 139.77832457969515 16.04224474 35.5383672990796 139.77832457490305 13.47224478</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2404_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2404_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2404_2">
											<gml:posList>35.53835729493377 139.7783318694182 16.04215233 35.53835729452812 139.77833186462908 13.47215237 35.5383672990796 139.77832457490305 13.47224478 35.53836729948119 139.77832457969515 16.04224474 35.53835729493377 139.7783318694182 16.04215233</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2404_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2404_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2404_3">
											<gml:posList>35.53834262205491 139.77829944941416 16.04267857 35.5383426216434 139.778299444612 13.4726786 35.53834321106328 139.77830074694594 13.47265743 35.53835729452812 139.77833186462908 13.47215237 35.53835729493377 139.7783318694182 16.04215233 35.53834262205491 139.77829944941416 16.04267857</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2404_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2404_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2404_4">
											<gml:posList>35.53835262660788 139.7782921596552 16.042771 35.538352626200414 139.77829215485008 13.47277104 35.5383426216434 139.778299444612 13.4726786 35.53834262205491 139.77829944941416 16.04267857 35.53835262660788 139.7782921596552 16.042771</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2403_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2403_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2403_1">
											<gml:posList>35.538585839602675 139.77822949962842 15.98833447 35.538585839296196 139.7782294949014 13.4733345 35.53857626773067 139.77820892134463 13.47366723 35.53857570956862 139.77820772160615 13.47368666 35.53857570987915 139.77820772634178 15.98868662 35.538585839602675 139.77822949962842 15.98833447</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2403_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2403_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2403_2">
											<gml:posList>35.53855518117289 139.77825116789484 15.98804675 35.53855518085425 139.77825116317644 13.47304678 35.538585839296196 139.7782294949014 13.4733345 35.538585839602675 139.77822949962842 15.98833447 35.53855518117289 139.77825116789484 15.98804675</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2403_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2403_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2403_3">
											<gml:posList>35.538545060312906 139.77822939416964 15.98839889 35.5385450599903 139.77822938944263 13.47339892 35.53854561024865 139.77823057325207 13.47337976 35.53855518085425 139.77825116317644 13.47304678 35.53855518117289 139.77825116789484 15.98804675 35.538545060312906 139.77822939416964 15.98839889</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2403_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2403_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2403_4">
											<gml:posList>35.53857570987915 139.77820772634178 15.98868662 35.53857570956862 139.77820772160615 13.47368666 35.5385450599903 139.77822938944263 13.47339892 35.538545060312906 139.77822939416964 15.98839889 35.53857570987915 139.77820772634178 15.98868662</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2402_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2402_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2402_1">
											<gml:posList>35.538399324835815 139.77836280411728 15.99159892 35.53839932445474 139.77836279943352 13.47159895 35.53838919795026 139.7783410394471 13.47195082 35.53838919833531 139.77834104413944 15.99195078 35.538399324835815 139.77836280411728 15.99159892</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2402_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2402_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2402_2">
											<gml:posList>35.53836866942961 139.7783844639157 15.99132611 35.538368669036366 139.77838445924047 13.47132615 35.53839932445474 139.77836279943352 13.47159895 35.538399324835815 139.77836280411728 15.99159892 35.53836866942961 139.7783844639157 15.99132611</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2402_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2402_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2402_3">
											<gml:posList>35.5383585360599 139.7783626993142 15.99167805 35.53835853566268 139.77836269463037 13.47167809 35.538368669036366 139.77838445924047 13.47132615 35.53836866942961 139.7783844639157 15.99132611 35.5383585360599 139.7783626993142 15.99167805</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2402_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2402_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2402_4">
											<gml:posList>35.53838919833531 139.77834104413944 15.99195078 35.53838919795026 139.7783410394471 13.47195082 35.53835853566268 139.77836269463037 13.47167809 35.5383585360599 139.7783626993142 15.99167805 35.53838919833531 139.77834104413944 15.99195078</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2420_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2420_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2420_1">
											<gml:posList>35.53925983148712 139.77773275290937 13.48060017 35.53925640217812 139.777724337625 13.48073636 35.53925055737596 139.77770999489462 13.48096873 35.53925055742331 139.77771000025766 16.21596869 35.53925983153052 139.77773275826257 16.21560013 35.53925983148712 139.77773275290937 13.48060017</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2420_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2420_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2420_2">
											<gml:posList>35.53923048208053 139.77775355261605 16.21527225 35.53923048202452 139.7777535472717 13.48027229 35.53925983148712 139.77773275290937 13.48060017 35.53925983153052 139.77773275826257 16.21560013 35.53923048208053 139.77775355261605 16.21527225</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2420_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2420_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2420_3">
											<gml:posList>35.53922119009895 139.77773079475043 16.21564073 35.53922119003897 139.77773078939634 13.48064077 35.53922704466998 139.77774512852156 13.4804085 35.53923048202452 139.7777535472717 13.48027229 35.53923048208053 139.77775355261605 16.21527225 35.53922119009895 139.77773079475043 16.21564073</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2420_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2420_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2420_4">
											<gml:posList>35.53925055742331 139.77771000025766 16.21596869 35.53925055737596 139.77770999489462 13.48096873 35.53922119003897 139.77773078939634 13.48064077 35.53922119009895 139.77773079475043 16.21564073 35.53925055742331 139.77771000025766 16.21596869</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2414_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2414_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2414_1">
											<gml:posList>35.53998675900735 139.77723434452767 13.48915252 35.539976917929465 139.7772119863115 13.48951525 35.539976917629446 139.7772119926218 16.58451519 35.539986758702554 139.77723435082714 16.58415247 35.53998675900735 139.77723434452767 13.48915252</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2414_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2414_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2414_2">
											<gml:posList>35.53995741219145 139.7772551535692 16.5837698 35.53995741248193 139.77725514727965 13.48876985 35.53998675900735 139.77723434452767 13.48915252 35.539986758702554 139.77723435082714 16.58415247 35.53995741219145 139.7772551535692 16.5837698</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2414_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2414_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2414_3">
											<gml:posList>35.53994756504626 139.77723278400666 16.58413267 35.539947565331964 139.77723277770642 13.48913272 35.53994826172322 139.7772343596839 13.48910703 35.53995741248193 139.77725514727965 13.48876985 35.53995741219145 139.7772551535692 16.5837698 35.53994756504626 139.77723278400666 16.58413267</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2414_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2414_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2414_4">
											<gml:posList>35.539976917629446 139.7772119926218 16.58451519 35.539976917929465 139.7772119863115 13.48951525 35.539947565331964 139.77723277770642 13.48913272 35.53994756504626 139.77723278400666 16.58413267 35.539976917629446 139.7772119926218 16.58451519</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2413_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2413_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2413_1">
											<gml:posList>35.539798833600535 139.77736724616034 13.48674568 35.539790276731125 139.7773462462134 13.48708669 35.53978956025446 139.77734448786384 13.48711528 35.539789560070965 139.7773444933532 16.20711523 35.53979883341306 139.77736725164 16.20674564 35.539798833600535 139.77736724616034 13.48674568</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2413_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2413_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2413_2">
											<gml:posList>35.539769484128165 139.7773880461594 16.20637734 35.539769484303115 139.7773880406887 13.48637738 35.539798833600535 139.77736724616034 13.48674568 35.53979883341306 139.77736725164 16.20674564 35.539769484128165 139.7773880461594 16.20637734</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2413_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2413_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2413_3">
											<gml:posList>35.53976019291223 139.77736528801225 16.20674683 35.53976019308321 139.77736528253172 13.48674688 35.539760261671276 139.777365450533 13.48674415 35.539761148775746 139.77736762342914 13.48670883 35.539769484303115 139.7773880406887 13.48637738 35.539769484128165 139.7773880461594 16.20637734 35.53976019291223 139.77736528801225 16.20674683</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2413_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2413_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2413_4">
											<gml:posList>35.539789560070965 139.7773444933532 16.20711523 35.53978956025446 139.77734448786384 13.48711528 35.53976019308321 139.77736528253172 13.48674688 35.53976019291223 139.77736528801225 16.20674683 35.539789560070965 139.7773444933532 16.20711523</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2412_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2412_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2412_1">
											<gml:posList>35.539614190596 139.777500718429 13.48442222 35.539605000502036 139.77747820858895 13.48478717 35.539604899154966 139.77747796035376 13.4847912 35.53960489905031 139.7774779657963 16.20979116 35.53961419048737 139.77750072386175 16.20942217 35.539614190596 139.777500718429 13.48442222</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2412_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2412_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2412_2">
											<gml:posList>35.53958482876317 139.777521530231 16.20906774 35.539584828859276 139.77752152480713 13.48406779 35.539614190596 139.777500718429 13.48442222 35.53961419048737 139.77750072386175 16.20942217 35.53958482876317 139.777521530231 16.20906774</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2412_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2412_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2412_3">
											<gml:posList>35.539575560478404 139.7774987620549 16.20943682 35.53957556057053 139.77749875662133 13.48443687 35.53957565582234 139.77749899061388 13.48443307 35.539584828859276 139.77752152480713 13.48406779 35.53958482876317 139.777521530231 16.20906774 35.539575560478404 139.7774987620549 16.20943682</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2412_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2412_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2412_4">
											<gml:posList>35.53960489905031 139.7774779657963 16.20979116 35.539604899154966 139.77747796035376 13.4847912 35.53957556057053 139.77749875662133 13.48443687 35.539575560478404 139.7774987620549 16.20943682 35.53960489905031 139.7774779657963 16.20979116</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2411_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2411_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2411_1">
											<gml:posList>35.539426221443 139.77763415156465 16.26718795 35.53942622147174 139.77763414607054 13.482188 35.53941694424776 139.77761139050702 13.48255642 35.53941694422309 139.77761139601117 16.26755637 35.539426221443 139.77763415156465 16.26718795</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2411_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2411_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2411_2">
											<gml:posList>35.539396869702024 139.77765494275909 16.26684803 35.539396869717876 139.77765493727404 13.48184807 35.53942622147174 139.77763414607054 13.482188 35.539426221443 139.77763415156465 16.26718795 35.539396869702024 139.77765494275909 16.26684803</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2411_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2411_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2411_3">
											<gml:posList>35.53938758341139 139.7776321893469 16.26721633 35.53938758342328 139.7776321838519 13.48221637 35.539396869717876 139.77765493727404 13.48184807 35.539396869702024 139.77765494275909 16.26684803 35.53938758341139 139.7776321893469 16.26721633</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2411_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2411_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2411_4">
											<gml:posList>35.53941694422309 139.77761139601117 16.26755637 35.53941694424776 139.77761139050702 13.48255642 35.53938758342328 139.7776321838519 13.48221637 35.53938758341139 139.7776321893469 16.26721633 35.53941694422309 139.77761139601117 16.26755637</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2410_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2410_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2410_1">
											<gml:posList>35.539333850224274 139.77769881460688 13.48113806 35.539324809749 139.77767664012725 13.48149683 35.53932457292458 139.77767605924456 13.48150624 35.539324572940444 139.77767606473023 16.2715062 35.53933385023608 139.7776988200825 16.27113802 35.539333850224274 139.77769881460688 13.48113806</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2410_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2410_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2410_2">
											<gml:posList>35.53930449866674 139.7777196249749 16.27080485 35.539304498642046 139.77771961950822 13.4808049 35.539333850224274 139.77769881460688 13.48113806 35.53933385023608 139.7776988200825 16.27113802 35.53930449866674 139.7777196249749 16.27080485</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2410_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2410_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2410_3">
											<gml:posList>35.53929521230463 139.77769685788303 16.27117314 35.53929521227595 139.77769685240642 13.48117319 35.539304498642046 139.77771961950822 13.4808049 35.53930449866674 139.7777196249749 16.27080485 35.53929521230463 139.77769685788303 16.27117314</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2410_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2410_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2410_4">
											<gml:posList>35.539324572940444 139.77767606473023 16.2715062 35.53932457292458 139.77767605924456 13.48150624 35.53929521227595 139.77769685240642 13.48117319 35.53929521230463 139.77769685788303 16.27117314 35.539324572940444 139.77767606473023 16.2715062</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2419_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2419_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2419_1">
											<gml:posList>35.53930172261164 139.77765844318307 13.48179339 35.53928228436174 139.77761515298934 13.48249815 35.539281214335965 139.77761276997688 13.48253704 35.53928121437418 139.77761277602377 16.54253699 35.539301722640026 139.7776584492081 16.54179334 35.53930172261164 139.77765844318307 13.48179339</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2419_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2419_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2419_2">
											<gml:posList>35.539291868825806 139.77766561599802 16.54167855 35.53929186879265 139.77766560997637 13.48167859 35.53930172261164 139.77765844318307 13.48179339 35.539301722640026 139.7776584492081 16.54179334 35.539291868825806 139.77766561599802 16.54167855</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2419_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2419_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2419_3">
											<gml:posList>35.53927136540149 139.77761993304642 16.54242232 35.539271365358495 139.77761992700297 13.48242237 35.53929186879265 139.77766560997637 13.48167859 35.539291868825806 139.77766561599802 16.54167855 35.53927136540149 139.77761993304642 16.54242232</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2419_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2419_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2419_4">
											<gml:posList>35.53928121437418 139.77761277602377 16.54253699 35.539281214335965 139.77761276997688 13.48253704 35.539271365358495 139.77761992700297 13.48242237 35.53927136540149 139.77761993304642 16.54242232 35.53928121437418 139.77761277602377 16.54253699</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2418_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2418_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2418_1">
											<gml:posList>35.53937417373123 139.77760896730476 16.53759296 35.539374173737734 139.7776089612659 13.48259301 35.53935384435446 139.77756366507532 13.48333054 35.53935367068287 139.77756327811565 13.48333686 35.53935367068632 139.77756328417638 16.53833681 35.53937417373123 139.77760896730476 16.53759296</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2418_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2418_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2418_2">
											<gml:posList>35.53936430705442 139.77761612438303 16.53747649 35.53936430705615 139.7776161183476 13.48247653 35.539374173737734 139.7776089612659 13.48259301 35.53937417373123 139.77760896730476 16.53759296 35.53936430705442 139.77761612438303 16.53747649</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2418_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2418_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2418_3">
											<gml:posList>35.53934382179364 139.7775704411571 16.5382203 35.53934382178563 139.77757043509982 13.48322035 35.53936430705615 139.7776161183476 13.48247653 35.53936430705442 139.77761612438303 16.53747649 35.53934382179364 139.7775704411571 16.5382203</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2418_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2418_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2418_4">
											<gml:posList>35.53935367068632 139.77756328417638 16.53833681 35.53935367068287 139.77756327811565 13.48333686 35.53934382178563 139.77757043509982 13.48322035 35.53934382179364 139.7775704411571 16.5382203 35.53935367068632 139.77756328417638 16.53833681</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2417_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2417_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2417_1">
											<gml:posList>35.53939421634771 139.7775949413037 13.48282196 35.53937583945698 139.77755399526347 13.48348854 35.53937371337727 139.77754925809165 13.48356584 35.53937371337098 139.777549264159 16.53856579 35.53939421633157 139.77759494734917 16.53782191 35.53939421634771 139.7775949413037 13.48282196</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2417_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2417_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2417_2">
											<gml:posList>35.53938436751686 139.77760210432476 16.53770492 35.539384367528214 139.77760209828256 13.48270497 35.53939421634771 139.7775949413037 13.48282196 35.53939421633157 139.77759494734917 16.53782191 35.53938436751686 139.77760210432476 16.53770492</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2417_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2417_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2417_3">
											<gml:posList>35.53936385942974 139.7775564092307 16.53844896 35.53936385943126 139.77755640316678 13.48344901 35.539384367528214 139.77760209828256 13.48270497 35.53938436751686 139.77760210432476 16.53770492 35.53936385942974 139.7775564092307 16.53844896</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2417_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2417_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2417_4">
											<gml:posList>35.53937371337098 139.777549264159 16.53856579 35.53937371337727 139.77754925809165 13.48356584 35.53936385943126 139.77755640316678 13.48344901 35.53936385942974 139.7775564092307 16.53844896 35.53937371337098 139.777549264159 16.53856579</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2416_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2416_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2416_1">
											<gml:posList>35.53958249748588 139.7774644184685 13.48500363 35.53957651587542 139.7774510900979 13.48522036 35.53956198996882 139.77741872313996 13.48574791 35.53956198987223 139.77741872925978 16.53574786 35.539582497379456 139.77746442456655 16.53500358 35.53958249748588 139.7774644184685 13.48500363</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2416_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2416_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2416_2">
											<gml:posList>35.539572648780094 139.77747158142677 16.53488181 35.539572648881744 139.77747157533216 13.48488186 35.53958249748588 139.7774644184685 13.48500363 35.539582497379456 139.77746442456655 16.53500358 35.539572648780094 139.77747158142677 16.53488181</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2416_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2416_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2416_3">
											<gml:posList>35.53955214123127 139.77742588610295 16.53562605 35.53955214132318 139.7774258799866 13.4856261 35.539572648881744 139.77747157533216 13.48488186 35.539572648780094 139.77747158142677 16.53488181 35.53955214123127 139.77742588610295 16.53562605</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2416_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2416_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2416_4">
											<gml:posList>35.53956198987223 139.77741872925978 16.53574786 35.53956198996882 139.77741872313996 13.48574791 35.53955214132318 139.7774258799866 13.4856261 35.53955214123127 139.77742588610295 16.53562605 35.53956198987223 139.77741872925978 16.53574786</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2415_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2415_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2415_1">
											<gml:posList>35.53995987841444 139.77720252366584 13.48965392 35.5399428747117 139.77716165378763 13.49032132 35.53994287449742 139.77716165857714 15.83032128 35.53995987819393 139.77720252844026 15.82965387 35.53995987841444 139.77720252366584 13.48965392</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2415_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2415_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2415_2">
											<gml:posList>35.53994942305015 139.77721078257122 15.8295034 35.539949423266876 139.7772107777999 13.48950343 35.53995987841444 139.77720252366584 13.48965392 35.53995987819393 139.77720252844026 15.82965387 35.53994942305015 139.77721078257122 15.8295034</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2415_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2415_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2415_3">
											<gml:posList>35.53993241336409 139.7771699232832 15.83017052 35.5399324135746 139.77716991849675 13.49017056 35.539949423266876 139.7772107777999 13.48950343 35.53994942305015 139.77721078257122 15.8295034 35.53993241336409 139.7771699232832 15.83017052</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2415_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2415_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2415_4">
											<gml:posList>35.53994287449742 139.77716165857714 15.83032128 35.5399428747117 139.77716165378763 13.49032132 35.5399324135746 139.77716991849675 13.49017056 35.53993241336409 139.7771699232832 15.83017052 35.53994287449742 139.77716165857714 15.83032128</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2429_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_1">
											<gml:posList>35.53908346681376 139.7777246607725 14.02079503 35.539083466684886 139.77772465498484 11.06579507 35.53908320545401 139.77772409106416 11.0658043 35.53908320540706 139.77772408895856 9.99080431 35.53908167327084 139.7777207815267 9.99085842 35.5390816734885 139.7777207912443 14.95085834 35.53908853456961 139.77773560230457 14.95061618 35.53908853450653 139.7777355994225 13.47781507 35.53908853442274 139.77773559559245 11.52061624 35.53908613325895 139.77773041218376 11.52070094 35.539086133366844 139.777730417078 14.02070091 35.53908346681376 139.7777246607725 14.02079503</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2429_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_2">
											<gml:posList>35.53908355732425 139.77773918928833 14.25720368 35.539084722337016 139.77773834885195 13.99740399 35.539085887349955 139.77773750841547 13.7376043 35.539087052362966 139.77773666797896 13.47780461 35.53908853450653 139.7777355994225 13.47781507 35.53908853456961 139.77773560230457 14.95061618 35.539086666546076 139.77773694906247 14.9505955 35.53908355732425 139.77773918928833 14.25720368</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2429_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_3">
											<gml:posList>35.539085887349955 139.77773750841547 13.7376043 35.53908472231455 139.7777383478354 13.47778818 35.53908239226602 139.77774002769164 13.47777177 35.53908122725315 139.77774086812795 13.73757147 35.53908239228874 139.7777400287081 13.99738757 35.539084722337016 139.77773834885195 13.99740399 35.53908355732425 139.77773918928833 14.25720368 35.539086666546076 139.77773694906247 14.9505955 35.539077279571295 139.77774371663307 14.9504917 35.53907727941839 139.7777437099254 11.52049175 35.53908853442274 139.77773559559245 11.52061624 35.53908853450653 139.7777355994225 13.47781507 35.539087052362966 139.77773666797896 13.47780461 35.539085887349955 139.77773750841547 13.7376043</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2429_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_4">
											<gml:posList>35.539077279571295 139.77774371663307 14.9504917 35.539070426502704 139.7777288997839 14.95073394 35.5390704262763 139.77772889007275 9.99073401 35.539071934062726 139.7777321500196 9.99068069 35.539071934111554 139.7777321521239 11.06568067 35.53907216732157 139.77773265634139 11.06567242 35.53907216745566 139.77773266212535 14.02067237 35.53907483087895 139.7777384206463 14.02057822 35.53907483076655 139.77773841575527 11.52057826 35.53907727941839 139.7777437099254 11.52049175 35.539077279571295 139.77774371663307 14.9504917</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2429_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_5">
											<gml:posList>35.53908167327084 139.7777207815267 9.99085842 35.5390704262763 139.77772889007275 9.99073401 35.539070426502704 139.7777288997839 14.95073394 35.5390816734885 139.7777207912443 14.95085834 35.53908167327084 139.7777207815267 9.99085842</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2429_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_6">
											<gml:posList>35.53908320540053 139.77772408866477 9.84080431 35.53908320513112 139.7777240765759 3.66880441 35.539081672993355 139.77772076914064 3.66885852 35.539081673264214 139.7777207812328 9.84085842 35.53908320540053 139.77772408866477 9.84080431</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2429_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_7">
											<gml:posList>35.53907042626949 139.77772888977893 9.84073402 35.539081673264214 139.7777207812328 9.84085842 35.539081672993355 139.77772076914064 3.66885852 35.53907042598773 139.77772887769473 3.66873412 35.53907042626949 139.77772888977893 9.84073402</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2429_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2429_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2429_8">
											<gml:posList>35.539071933775595 139.77773213764493 3.66868079 35.53907193405592 139.77773214972606 9.84068069 35.53907042626949 139.77772888977893 9.84073402 35.53907042598773 139.77772887769473 3.66873412 35.539071933775595 139.77773213764493 3.66868079</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2426_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2426_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2426_2">
											<gml:posList>35.53915857516131 139.77776798808756 11.52006346 35.53915857523719 139.77776799274264 13.90506343 35.53917038232445 139.77779349841194 13.90464996 35.53917038225308 139.77779349376644 11.51965 35.53915857516131 139.77776798808756 11.52006346</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2426_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2426_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2426_3">
											<gml:posList>35.53912461402403 139.777825311754 13.48976656 35.53912226303359 139.7778269459578 13.48974387 35.53912108754814 139.7778277635643 13.74954043 35.53912178695962 139.77782727769875 13.90412979 35.539052861049065 139.77787518893743 13.90340237 35.53905286093364 139.7778751843224 11.5184024 35.53917038225308 139.77779349376644 11.51965 35.53917038232445 139.77779349841194 13.90464996 35.53912509011407 139.77782498163347 13.90416496 35.53912578952886 139.7778244951569 13.74958582 35.53912461402403 139.777825311754 13.48976656</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2426_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2426_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2426_4">
											<gml:posList>35.53905286093364 139.7778751843224 11.5184024 35.539052861049065 139.77787518893743 13.90340237 35.539041045283334 139.77784968136078 13.90381587 35.539041045163486 139.77784967673625 11.51881591 35.53905286093364 139.7778751843224 11.5184024</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2426_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2426_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2426_5">
											<gml:posList>35.539041045283334 139.77784968136078 13.90381587 35.53911007786231 139.77780170063895 13.90454435 35.53910936062074 139.77780219884144 13.74601782 35.539110536133265 139.7778013812941 13.48622127 35.53911288717887 139.77779974721085 13.48624398 35.53911406271177 139.77779893067492 13.74606325 35.53911334546807 139.77779942950485 13.90457914 35.53915857523719 139.77776799274264 13.90506343 35.53915857516131 139.77776798808756 11.52006346 35.539041045163486 139.77784967673625 11.51881591 35.539041045283334 139.77784968136078 13.90381587</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2423_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2423_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2423_2">
											<gml:posList>35.539161624375026 139.77766800506132 9.99167355 35.53908320540706 139.77772408895856 9.99080431 35.53908320545401 139.77772409106416 11.0658043 35.53916162440871 139.7776680071764 11.06667354 35.539161624375026 139.77766800506132 9.99167355</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2423_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2423_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2423_3">
											<gml:posList>35.539071934062726 139.7777321500196 9.99068069 35.53899265828652 139.77778884654208 9.98982049 35.5389926583487 139.77778884863673 11.06482047 35.539071934111554 139.7777321521239 11.06568067 35.539071934062726 139.7777321500196 9.99068069</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2423_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2423_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2423_4">
											<gml:posList>35.53908320540053 139.77772408866477 9.84080431 35.53916162437037 139.7776680047662 9.84167356 35.5391616241771 139.777667992623 3.66967366 35.53908320513112 139.7777240765759 3.66880441 35.53908320540053 139.77772408866477 9.84080431</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2423_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2423_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2423_5">
											<gml:posList>35.53899265827783 139.77778884624985 9.83982049 35.53907193405592 139.77773214972606 9.84068069 35.539071933775595 139.77773213764493 3.66868079 35.53899265792056 139.7777888342235 3.66782059 35.53899265827783 139.77778884624985 9.83982049</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2422_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2422_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2422_1">
											<gml:posList>35.53822109253841 139.77832807721822 9.83249294 35.53822109256522 139.77832807749778 9.98249294 35.53821315236047 139.77831135247763 9.9827671 35.538213152333384 139.77831135219768 9.8327671 35.53822109253841 139.77832807721822 9.83249294</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2422_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2422_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2422_3">
											<gml:posList>35.53821315236047 139.77831135247763 9.9827671 35.53997697247093 139.7770579245099 10.00211373 35.53997697248547 139.77705792420045 9.85211373 35.538213152333384 139.77831135219768 9.8327671 35.53821315236047 139.77831135247763 9.9827671</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2422_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2422_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2422_4">
											<gml:posList>35.539985892619896 139.77707671482565 10.00180589 35.53998589263471 139.77707671451665 9.85180589 35.53997697248547 139.77705792420045 9.85211373 35.53997697247093 139.7770579245099 10.00211373 35.539985892619896 139.77707671482565 10.00180589</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2421_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_5">
											<gml:posList>35.53916207508958 139.77766899037064 11.06665746 35.539083466684886 139.77772465498484 11.06579507 35.53908346681376 139.7777246607725 14.02079503 35.539162075181856 139.77766899618413 14.02165741 35.53916207508958 139.77766899037064 11.06665746</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2421_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_6">
											<gml:posList>35.53899269638008 139.7777889315917 11.06481912 35.5389926965512 139.77778893734964 14.01981907 35.53907216745566 139.77773266212535 14.02067237 35.53907216732157 139.77773265634139 11.06567242 35.53899269638008 139.7777889315917 11.06481912</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2421_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_7">
											<gml:posList>35.53907483076655 139.77773841575527 11.52057826 35.53907483087895 139.7777384206463 14.02057822 35.538995342321826 139.77779470836518 14.01972474 35.538995342178154 139.7777947034961 11.51972477 35.53907483076655 139.77773841575527 11.52057826</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2421_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2421_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2421_8">
											<gml:posList>35.539086133366844 139.777730417078 14.02070091 35.53908613325895 139.77773041218376 11.52070094 35.53916472055649 139.77767476252157 11.52156309 35.539164720633586 139.7776747674377 14.02156305 35.539086133366844 139.777730417078 14.02070091</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2424_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2424_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2424_1">
											<gml:posList>35.53917856734687 139.7777211071054 11.52080995 35.53916743688085 139.7776968281189 11.52120555 35.539167436969294 139.77769683383102 14.4312055 35.53917856743027 139.77772111280652 14.4308099 35.53917856734687 139.7777211071054 11.52080995</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2424_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2424_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2424_2">
											<gml:posList>35.53910914526346 139.77776836277903 14.4300792 35.53910914514835 139.77776835709943 11.52007925 35.53917856734687 139.7777211071054 11.52080995 35.53917856743027 139.77772111280652 14.4308099 35.53910914526346 139.77776836277903 14.4300792</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2424_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2424_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2424_3">
											<gml:posList>35.53909801512288 139.7777440699524 14.43047504 35.53909801500271 139.7777440642617 11.52047509 35.53910253316926 139.77775392568384 11.52031428 35.53910914514835 139.77776835709943 11.52007925 35.53910914526346 139.77776836277903 14.4300792 35.53909801512288 139.7777440699524 14.43047504</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2424_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2424_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2424_4">
											<gml:posList>35.53909801512288 139.7777440699524 14.43047504 35.539167436969294 139.77769683383102 14.4312055 35.53916743688085 139.7776968281189 11.52120555 35.53909801500271 139.7777440642617 11.52047509 35.53909801512288 139.7777440699524 14.43047504</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2425_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2425_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2425_1">
											<gml:posList>35.539081701417146 139.77779324704753 13.76969427 35.53908170131841 139.77779324266487 11.51969431 35.53907296711235 139.77777440443546 11.52000109 35.53907296721415 139.7777744088247 13.77000106 35.539081701417146 139.77779324704753 13.76969427</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2425_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2425_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2425_2">
											<gml:posList>35.539026073767666 139.77783178088777 13.769111 35.53902607364918 139.77783177651878 11.51911104 35.53908170131841 139.77779324266487 11.51969431 35.539081701417146 139.77779324704753 13.76969427 35.539026073767666 139.77783178088777 13.769111</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2425_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2425_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2425_3">
											<gml:posList>35.539017348367665 139.777812928377 11.51941798 35.53902607364918 139.77783177651878 11.51911104 35.539026073767666 139.77783178088777 13.769111 35.5390173484892 139.7778129327527 13.76941795 35.539017348367665 139.777812928377 11.51941798</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2425_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2425_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2425_4">
											<gml:posList>35.53907296711235 139.77777440443546 11.52000109 35.539017348367665 139.777812928377 11.51941798 35.5390173484892 139.7778129327527 13.76941795 35.53907296721415 139.7777744088247 13.77000106 35.53907296711235 139.77777440443546 11.52000109</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2427_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2427_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2427_1">
											<gml:posList>35.53919484340231 139.7777697619215 11.52002207 35.53918371038936 139.7777454773849 11.52041589 35.53918371045728 139.77774548215592 13.96041585 35.53919484346598 139.7777697666832 13.96002204 35.53919484340231 139.7777697619215 11.52002207</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2427_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2427_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2427_2">
											<gml:posList>35.53918421467638 139.77777706336607 11.51990845 35.53919484340231 139.7777697619215 11.52002207 35.53919484346598 139.7777697666832 13.96002204 35.539184214744104 139.77777706812492 13.95990841 35.53918421467638 139.77777706336607 11.51990845</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2427_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2427_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2427_3">
											<gml:posList>35.53917308164505 139.77775277882841 11.52030227 35.53917486396553 139.7777566666121 11.52023915 35.53918421467638 139.77777706336607 11.51990845 35.539184214744104 139.77777706812492 13.95990841 35.53917308171702 139.77775278359667 13.96030223 35.53917308164505 139.77775277882841 11.52030227</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0275_p2427_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0275_p2427_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0275_p2427_4">
											<gml:posList>35.53918371038936 139.7777454773849 11.52041589 35.53917308164505 139.77775277882841 11.52030227 35.53917308171702 139.77775278359667 13.96030223 35.53918371045728 139.77774548215592 13.96041585 35.53918371038936 139.7777454773849 11.52041589</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:address>
				<core:Address>
					<core:xalAddress>
						<xAL:AddressDetails>
							<xAL:Country>
								<xAL:CountryName>日本</xAL:CountryName>
								<xAL:Locality>
									<xAL:LocalityName Type="Town">東京都大田区羽田空港二丁目</xAL:LocalityName>
								</xAL:Locality>
							</xAL:Country>
						</xAL:AddressDetails>
					</core:xalAddress>
				</core:Address>
			</bldg:address>
			<uro:buildingDetails>
				<uro:BuildingDetails>
					<uro:buildingRoofEdgeArea uom="m2">19883.21715</uro:buildingRoofEdgeArea>
					<uro:districtsAndZonesType codeSpace="../../codelists/Common_districtsAndZonesType.xml">11</uro:districtsAndZonesType>
					<uro:prefecture codeSpace="../../codelists/Common_prefecture.xml">13</uro:prefecture>
					<uro:city codeSpace="../../codelists/Common_localPublicAuthorities.xml">13111</uro:city>
					<uro:surveyYear>2016</uro:surveyYear>
				</uro:BuildingDetails>
			</uro:buildingDetails>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">2</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key2.xml">2</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">106</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key106.xml">20</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
		</bldg:Building>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<bldg:Building gml:id="BLD_abe3af3a-a271-4a76-a3f0-3a84aa48eb04">
			<gen:stringAttribute name="建物ID">
				<gen:value>13111-bldg-147300</gen:value>
			</gen:stringAttribute>
			<bldg:measuredHeight uom="m">9.3</bldg:measuredHeight>
			<bldg:lod0RoofEdge>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>35.54007723493803 139.776986327635 12.369 35.53999505340527 139.77704478356958 12.369 35.539989525195324 139.7770487157324 12.369 35.540075703391416 139.77722976448365 12.369 35.54007451394983 139.77723061780475 12.369 35.54023526410485 139.77757130295203 12.369 35.540439490100034 139.77800409426007 12.369 35.541529536894586 139.77722909612166 12.369 35.54116489784815 139.77645643300562 12.369 35.54116546823812 139.77645602675352 12.369 35.54115623383091 139.77643386154534 12.369 35.54114781506271 139.77644010080775 12.369 35.541145553912656 139.7764353105152 12.369 35.541071751513414 139.77627892330986 12.369 35.54106945914231 139.77628055494617 12.369 35.54096637358333 139.7763538813832 12.369 35.54007723493803 139.776986327635 12.369</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</bldg:lod0RoofEdge>
			<bldg:lod1Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54007723493803 139.776986327635 3.348 35.54096637358333 139.7763538813832 3.348 35.54106945914231 139.77628055494617 3.348 35.541071751513414 139.77627892330986 3.348 35.541145553912656 139.7764353105152 3.348 35.54114781506271 139.77644010080775 3.348 35.54115623383091 139.77643386154534 3.348 35.54116546823812 139.77645602675352 3.348 35.54116489784815 139.77645643300562 3.348 35.541529536894586 139.77722909612166 3.348 35.540439490100034 139.77800409426007 3.348 35.54023526410485 139.77757130295203 3.348 35.54007451394983 139.77723061780475 3.348 35.540075703391416 139.77722976448365 3.348 35.539989525195324 139.7770487157324 3.348 35.53999505340527 139.77704478356958 3.348 35.54007723493803 139.776986327635 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54007723493803 139.776986327635 3.348 35.53999505340527 139.77704478356958 3.348 35.53999505340527 139.77704478356958 12.369 35.54007723493803 139.776986327635 12.369 35.54007723493803 139.776986327635 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53999505340527 139.77704478356958 3.348 35.539989525195324 139.7770487157324 3.348 35.539989525195324 139.7770487157324 12.369 35.53999505340527 139.77704478356958 12.369 35.53999505340527 139.77704478356958 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.539989525195324 139.7770487157324 3.348 35.540075703391416 139.77722976448365 3.348 35.540075703391416 139.77722976448365 12.369 35.539989525195324 139.7770487157324 12.369 35.539989525195324 139.7770487157324 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.540075703391416 139.77722976448365 3.348 35.54007451394983 139.77723061780475 3.348 35.54007451394983 139.77723061780475 12.369 35.540075703391416 139.77722976448365 12.369 35.540075703391416 139.77722976448365 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54007451394983 139.77723061780475 3.348 35.54023526410485 139.77757130295203 3.348 35.54023526410485 139.77757130295203 12.369 35.54007451394983 139.77723061780475 12.369 35.54007451394983 139.77723061780475 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54023526410485 139.77757130295203 3.348 35.540439490100034 139.77800409426007 3.348 35.540439490100034 139.77800409426007 12.369 35.54023526410485 139.77757130295203 12.369 35.54023526410485 139.77757130295203 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.540439490100034 139.77800409426007 3.348 35.541529536894586 139.77722909612166 3.348 35.541529536894586 139.77722909612166 12.369 35.540439490100034 139.77800409426007 12.369 35.540439490100034 139.77800409426007 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541529536894586 139.77722909612166 3.348 35.54116489784815 139.77645643300562 3.348 35.54116489784815 139.77645643300562 12.369 35.541529536894586 139.77722909612166 12.369 35.541529536894586 139.77722909612166 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54116489784815 139.77645643300562 3.348 35.54116546823812 139.77645602675352 3.348 35.54116546823812 139.77645602675352 12.369 35.54116489784815 139.77645643300562 12.369 35.54116489784815 139.77645643300562 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54116546823812 139.77645602675352 3.348 35.54115623383091 139.77643386154534 3.348 35.54115623383091 139.77643386154534 12.369 35.54116546823812 139.77645602675352 12.369 35.54116546823812 139.77645602675352 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54115623383091 139.77643386154534 3.348 35.54114781506271 139.77644010080775 3.348 35.54114781506271 139.77644010080775 12.369 35.54115623383091 139.77643386154534 12.369 35.54115623383091 139.77643386154534 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54114781506271 139.77644010080775 3.348 35.541145553912656 139.7764353105152 3.348 35.541145553912656 139.7764353105152 12.369 35.54114781506271 139.77644010080775 12.369 35.54114781506271 139.77644010080775 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541145553912656 139.7764353105152 3.348 35.541071751513414 139.77627892330986 3.348 35.541071751513414 139.77627892330986 12.369 35.541145553912656 139.7764353105152 12.369 35.541145553912656 139.7764353105152 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541071751513414 139.77627892330986 3.348 35.54106945914231 139.77628055494617 3.348 35.54106945914231 139.77628055494617 12.369 35.541071751513414 139.77627892330986 12.369 35.541071751513414 139.77627892330986 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54106945914231 139.77628055494617 3.348 35.54096637358333 139.7763538813832 3.348 35.54096637358333 139.7763538813832 12.369 35.54106945914231 139.77628055494617 12.369 35.54106945914231 139.77628055494617 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54096637358333 139.7763538813832 3.348 35.54007723493803 139.776986327635 3.348 35.54007723493803 139.776986327635 12.369 35.54096637358333 139.7763538813832 12.369 35.54096637358333 139.7763538813832 3.348</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54007723493803 139.776986327635 12.369 35.53999505340527 139.77704478356958 12.369 35.539989525195324 139.7770487157324 12.369 35.540075703391416 139.77722976448365 12.369 35.54007451394983 139.77723061780475 12.369 35.54023526410485 139.77757130295203 12.369 35.540439490100034 139.77800409426007 12.369 35.541529536894586 139.77722909612166 12.369 35.54116489784815 139.77645643300562 12.369 35.54116546823812 139.77645602675352 12.369 35.54115623383091 139.77643386154534 12.369 35.54114781506271 139.77644010080775 12.369 35.541145553912656 139.7764353105152 12.369 35.541071751513414 139.77627892330986 12.369 35.54106945914231 139.77628055494617 12.369 35.54096637358333 139.7763538813832 12.369 35.54007723493803 139.776986327635 12.369</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod1Solid>
			<bldg:lod2Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_b_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_b_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_b_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_b_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_b_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_b_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2400_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2401_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2400_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2400_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2400_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2400_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2400_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2400_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2399_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_11"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_12"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2379_13"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2377_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2444_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2444_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2444_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2444_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2444_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2444_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2443_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2443_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2443_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2443_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2443_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2443_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2442_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2442_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2442_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2442_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2442_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2442_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2439_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2439_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2439_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2439_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2439_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0876_p2439_5"/>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod2Solid>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_150c7b77-1414-484d-9920-96673e3c8d58">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_b_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_b_0">
											<gml:posList>35.5410890863794 139.77647125674304 3.47281295 35.541091627112976 139.77647672316132 3.47272461 35.54104049076874 139.77651273010045 3.47191876 35.54104363203488 139.77651946321717 3.47181006 35.5409435923938 139.77658918013017 3.47026574 35.54093081819142 139.7765617997403 3.47070828 35.541030857948066 139.77649208273127 3.47225258 35.54103194252293 139.77649440745145 3.47221496 35.5410546415436 139.77647841997558 3.47257191 35.54105402882126 139.77647709952524 3.47259329 35.54110402940008 139.7764416876802 3.47338773 35.54111066687505 139.77645597366063 3.47315661 35.5410890863794 139.77647125674304 3.47281295</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_a7051688-6784-4c6c-9ada-5773e5c3cf3d">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_b_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_b_1">
											<gml:posList>35.5411645376906 139.77645567009307 3.47334735 35.54152953793409 139.7772290876212 3.46141527 35.54043949186081 139.77800407912213 3.44539712 35.54007451431614 139.7772306099995 3.45732871 35.5411645376906 139.77645567009307 3.47334735</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_59c08315-9f4a-49a7-b089-2ffcd332f4d8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_b_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_b_2">
											<gml:posList>35.53999583493701 139.7770487100423 3.46029285 35.53999661554801 139.77704926127709 3.46028446 35.53999661566857 139.77705036400195 3.46026573 35.53999583517803 139.77705091549214 3.46025541 35.539995054567044 139.77705036425738 3.46026381 35.539995054446486 139.77704926153254 3.46028253 35.53999583493701 139.7770487100423 3.46029285</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_3509b036-d33c-4ab9-9cae-a2ac790a4711">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_b_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_b_3">
											<gml:posList>35.54107024159097 139.77628481497229 3.47607654 35.54107102220533 139.776285366207 3.47606922 35.541071022332886 139.77628646894667 3.47604942 35.54107024184599 139.77628702045152 3.47603693 35.54106946123165 139.7762864692169 3.47604425 35.54106946110418 139.77628536647725 3.47606405 35.54107024159097 139.77628481497229 3.47607654</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_01822877-ec09-47c8-b481-517d8cc16bef">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_b_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_b_4">
											<gml:posList>35.54114633778485 139.77644610603852 3.47345383 35.54114711839846 139.7764466572753 3.47344675 35.54114711852455 139.77644776001603 3.47342717 35.541146338037024 139.77644831151986 3.47341468 35.54114555742333 139.77644776028305 3.47342177 35.541145557297334 139.77644665754246 3.47344135 35.54114633778485 139.77644610603852 3.47345383</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_4f56b2ae-5428-4818-9972-53385c20b586">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_b_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_b_5">
											<gml:posList>35.54007804194694 139.77722144726667 3.45748707 35.54007882255714 139.77722199850356 3.45747892 35.54007882267604 139.77722310122968 3.45746044 35.5400780421848 139.77722365271876 3.45745012 35.540077261574616 139.77722310148175 3.45745827 35.540077261455636 139.77722199875575 3.45747675 35.54007804194694 139.77722144726667 3.45748707</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:OuterCeilingSurface gml:id="ceil_HNAP0876_p2379_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_3">
											<gml:posList>35.54094359090063 139.7765891929426 9.46726563 35.541043630447284 139.77651947609505 9.46880994 35.54105139553922 139.77653612011292 9.46854157 35.54007726149768 139.77722866868353 9.45436513 35.540077260898656 139.77722311369857 9.45445817 35.54007804150814 139.77722366493504 9.45445002 35.54007882199864 139.7772231134465 9.45446034 35.54007882187966 139.7772220107215 9.45447882 35.54007804127018 139.77722145948502 9.45448697 35.54007726077968 139.77722201097356 9.45447664 35.540077235126056 139.776986327996 9.45846108 35.54096637352936 139.77635388087933 9.47150585 35.541030856372565 139.77649209563484 9.46925246 35.54093081671035 139.7765618125784 9.46770817 35.54094359090063 139.7765891929426 9.46726563</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:OuterCeilingSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:OuterCeilingSurface gml:id="ceil_HNAP0876_p2379_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_4">
											<gml:posList>35.5399895250104 139.77704871560493 9.45728514 35.53999505335721 139.7770447833342 9.45735867 35.53999505384806 139.77704927391258 9.45728243 35.53999505396862 139.77705037663648 9.45726371 35.5399950550564 139.7770603334051 9.45709475 35.5399895250104 139.77704871560493 9.45728514</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:OuterCeilingSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:OuterCeilingSurface gml:id="ceil_HNAP0876_p2379_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_5">
											<gml:posList>35.53999505335721 139.7770447833342 9.45735867 35.540077235126056 139.776986327996 9.45846108 35.54007726077968 139.77722201097356 9.45447664 35.540077260898656 139.77722311369857 9.45445817 35.54007726149768 139.77722866868353 9.45436513 35.54007570781268 139.77722977324106 9.45434448 35.5399950550564 139.7770603334051 9.45709475 35.53999505396862 139.77705037663648 9.45726371 35.53999583457888 139.7770509278707 9.45725531 35.53999661506861 139.77705037638106 9.45726563 35.539996614948144 139.77704927365718 9.45728435 35.53999583433786 139.77704872242296 9.45729275 35.53999505384806 139.77704927391258 9.45728243 35.53999505335721 139.7770447833342 9.45735867</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:OuterCeilingSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:OuterCeilingSurface gml:id="ceil_HNAP0876_p2379_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_6">
											<gml:posList>35.54096637352936 139.77635388087933 9.47150585 35.54106945893421 139.77628055482467 9.4731506 35.541069459492284 139.77628537957494 9.47306393 35.54106945961975 139.7762864823135 9.47304413 35.541069480262834 139.77646616816256 9.46983801 35.54105402722386 139.77647711244282 9.46959318 35.541054639945656 139.77647843289196 9.4695718 35.541031940946446 139.77649442035283 9.46921484 35.541030856372565 139.77649209563484 9.46925246 35.54096637352936 139.77635388087933 9.47150585</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:OuterCeilingSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:OuterCeilingSurface gml:id="ceil_HNAP0876_p2379_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_7">
											<gml:posList>35.541043630447284 139.77651947609505 9.46880994 35.5410404891842 139.77651274298464 9.46891864 35.54109162548015 139.7764767360794 9.46972449 35.541089084748926 139.7764712696662 9.46981283 35.54111066522422 139.77645598659814 9.47015649 35.54110402775555 139.77644170063115 9.47038762 35.541069480262834 139.77646616816256 9.46983801 35.54106945961975 139.7762864823135 9.47304413 35.541070240233374 139.77628703354767 9.47303681 35.54107102071946 139.77628648204328 9.4730493 35.54107102059199 139.77628537930468 9.4730691 35.54107023997836 139.77628482807054 9.47307642 35.541069459492284 139.77628537957494 9.47306393 35.54106945893421 139.77628055482467 9.4731506 35.54107175190935 139.7762789237977 9.4731875 35.541145554314646 139.77643531084433 9.47064297 35.541145555613596 139.7764466704887 9.47044123 35.541145555739675 139.77644777322828 9.47042165 35.541145558183864 139.77646917529958 9.47004204 35.54105139553922 139.77653612011292 9.46854157 35.541043630447284 139.77651947609505 9.46880994</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:OuterCeilingSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:OuterCeilingSurface gml:id="ceil_HNAP0876_p2379_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_8">
											<gml:posList>35.541164891701065 139.77645643675856 9.4703351 35.54116453598901 139.77645568303083 9.47034723 35.541145558183864 139.77646917529958 9.47004204 35.541145555739675 139.77644777322828 9.47042165 35.54114633635256 139.77644832446455 9.47041457 35.54114711683936 139.77644777296118 9.47042705 35.54114711671338 139.77644667022156 9.47044663 35.54114633610049 139.77644611898532 9.47045372 35.541145555613596 139.7764466704887 9.47044123 35.541145554314646 139.77643531084433 9.47064297 35.541147814800404 139.77644010081733 9.4705657 35.541156233664026 139.77643386164198 9.47070574 35.541165468027444 139.7764560267104 9.47034439 35.541164891701065 139.77645643675856 9.4703351</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:OuterCeilingSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2401_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_0">
											<gml:posList>35.54105463915293 139.77647843929952 12.44457174 35.5410830972163 139.77645839557383 12.44502114 35.54108908394007 139.77647127607707 12.44481278 35.54111066440527 139.7764559930161 12.44515643 35.54110402693967 139.77644170705582 12.44538756 35.5410540264314 139.77647711885106 12.44459312 35.54105463915293 139.77647843929952 12.44457174</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2400_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2400_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2400_1">
											<gml:posList>35.54108309721496 139.77645839558463 12.45002114 35.54103194016309 139.77649442676378 12.44921478 35.54104048839679 139.776512749387 12.44891858 35.541091624668766 139.77647674249855 12.44972443 35.54108309721496 139.77645839558463 12.45002114</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2399_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_1">
											<gml:posList>35.54103085554495 139.77649210241253 12.6192524 35.540930815932285 139.77656181932176 12.6177081 35.54094359011634 139.7765891996724 12.61726557 35.54104362961343 139.7765194828593 12.61880988 35.54103085554495 139.77649210241253 12.6192524</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2379_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_0">
											<gml:posList>35.54007570779016 139.77722977364823 9.65434447 35.541051395486065 139.77653612054186 9.66854156 35.5410436303943 139.77651947652453 9.66880994 35.5409435908508 139.77658919336986 9.66726563 35.54093081666088 139.77656181300657 9.66770816 35.54103085631995 139.77649209606508 9.66925246 35.540966373478824 139.7763538813139 9.67150584 35.53998952499058 139.77704871601784 9.65728514 35.54007570779016 139.77722977364823 9.65434447</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2379_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_1">
											<gml:posList>35.54116546797068 139.7764560271419 9.67034439 35.54115623360753 139.7764338620741 9.67070574 35.541147814744185 139.77644010124925 9.67056569 35.541071751855476 139.77627892423467 9.67318749 35.540966373478824 139.7763538813139 9.67150584 35.54103085631995 139.77649209606508 9.66925246 35.54103194089381 139.77649442078308 9.66921484 35.54105463989241 139.77647843332278 9.66957179 35.54105402717062 139.77647711287366 9.66959317 35.541104027700676 139.77644170106305 9.67038761 35.541110665169164 139.77645598702952 9.67015649 35.5410890846946 139.77647127009723 9.66981283 35.54109162542573 139.7764767365102 9.66972449 35.541040489131305 139.77651274341434 9.66891864 35.5410436303943 139.77651947652453 9.66880994 35.541051395486065 139.77653612054186 9.66854156 35.54116453593224 139.77645568346227 9.67034723 35.54116489164431 139.77645643718992 9.6703351 35.54116546797068 139.7764560271419 9.67034439</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2377_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_1">
											<gml:posList>35.540439490474704 139.77800409476083 11.612397 35.54152953514769 139.77722910425135 11.62841512 35.540235264368334 139.77757130315788 12.25894653 35.540439490474704 139.77800409476083 11.612397</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2377_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_2">
											<gml:posList>35.54152953514769 139.77722910425135 11.62841512 35.54132409726014 139.77680102812522 12.2748793 35.540235264368334 139.77757130315788 12.25894653 35.54152953514769 139.77722910425135 11.62841512</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2377_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_3">
											<gml:posList>35.54007451339904 139.7772306266274 11.62432857 35.540235264368334 139.77757130315788 12.25894653 35.54132409726014 139.77680102812522 12.2748793 35.54007451339904 139.7772306266274 11.62432857</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2377_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_4">
											<gml:posList>35.54116453537332 139.77645568771229 11.64034719 35.54007451339904 139.7772306266274 11.62432857 35.54132409726014 139.77680102812522 12.2748793 35.54116453537332 139.77645568771229 11.64034719</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0876_p2377_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_5">
											<gml:posList>35.54132409726014 139.77680102812522 12.2748793 35.54152953514769 139.77722910425135 11.62841512 35.54116453537332 139.77645568771229 11.64034719 35.54132409726014 139.77680102812522 12.2748793</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2401_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_1">
											<gml:posList>35.5410890846946 139.77647127009723 9.66981283 35.541110665169164 139.77645598702952 9.67015649 35.54111066440527 139.7764559930161 12.44515643 35.54108908394007 139.77647127607707 12.44481278 35.5410890846946 139.77647127009723 9.66981283</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2401_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_2">
											<gml:posList>35.54105402717062 139.77647711287366 9.66959317 35.54105463989241 139.77647843332278 9.66957179 35.54105463915293 139.77647843929952 12.44457174 35.5410540264314 139.77647711885106 12.44459312 35.54105402717062 139.77647711287366 9.66959317</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2401_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_3">
											<gml:posList>35.541104027700676 139.77644170106305 9.67038761 35.54105402717062 139.77647711287366 9.66959317 35.5410540264314 139.77647711885106 12.44459312 35.54110402693967 139.77644170705582 12.44538756 35.541104027700676 139.77644170106305 9.67038761</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2401_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_4">
											<gml:posList>35.541110665169164 139.77645598702952 9.67015649 35.541104027700676 139.77644170106305 9.67038761 35.54110402693967 139.77644170705582 12.44538756 35.54111066440527 139.7764559930161 12.44515643 35.541110665169164 139.77645598702952 9.67015649</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2401_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_5">
											<gml:posList>35.54111066522422 139.77645598659814 9.47015649 35.541089084748926 139.7764712696662 9.46981283 35.5410890863794 139.77647125674304 3.47281295 35.54111066687505 139.77645597366063 3.47315661 35.54111066522422 139.77645598659814 9.47015649</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2401_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_6">
											<gml:posList>35.54110402775555 139.77644170063115 9.47038762 35.54111066522422 139.77645598659814 9.47015649 35.54111066687505 139.77645597366063 3.47315661 35.54110402940008 139.7764416876802 3.47338773 35.54110402775555 139.77644170063115 9.47038762</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2401_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_7">
											<gml:posList>35.541054639945656 139.77647843289196 9.4695718 35.54105402722386 139.77647711244282 9.46959318 35.54105402882126 139.77647709952524 3.47259329 35.5410546415436 139.77647841997558 3.47257191 35.541054639945656 139.77647843289196 9.4695718</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2401_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2401_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2401_8">
											<gml:posList>35.54105402722386 139.77647711244282 9.46959318 35.541069480262834 139.77646616816256 9.46983801 35.54110402775555 139.77644170063115 9.47038762 35.54110402940008 139.7764416876802 3.47338773 35.54105402882126 139.77647709952524 3.47259329 35.54105402722386 139.77647711244282 9.46959318</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2400_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2400_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2400_0">
											<gml:posList>35.5410830972163 139.77645839557383 12.44502114 35.54105463915293 139.77647843929952 12.44457174 35.54105463989241 139.77647843332278 9.66957179 35.54103194089381 139.77649442078308 9.66921484 35.54103194016309 139.77649442676378 12.44921478 35.54108309721496 139.77645839558463 12.45002114 35.5410830972163 139.77645839557383 12.44502114</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2400_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2400_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2400_2">
											<gml:posList>35.54104048839679 139.776512749387 12.44891858 35.541040489131305 139.77651274341434 9.66891864 35.54109162542573 139.7764767365102 9.66972449 35.541091624668766 139.77647674249855 12.44972443 35.54104048839679 139.776512749387 12.44891858</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2400_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2400_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2400_3">
											<gml:posList>35.54108309721496 139.77645839558463 12.45002114 35.541091624668766 139.77647674249855 12.44972443 35.54109162542573 139.7764767365102 9.66972449 35.5410890846946 139.77647127009723 9.66981283 35.54108908394007 139.77647127607707 12.44481278 35.5410830972163 139.77645839557383 12.44502114 35.54108309721496 139.77645839558463 12.45002114</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2400_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2400_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2400_4">
											<gml:posList>35.541054639945656 139.77647843289196 9.4695718 35.5410546415436 139.77647841997558 3.47257191 35.54103194252293 139.77649440745145 3.47221496 35.541031940946446 139.77649442035283 9.46921484 35.541054639945656 139.77647843289196 9.4695718</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2400_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2400_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2400_5">
											<gml:posList>35.5410890863794 139.77647125674304 3.47281295 35.541089084748926 139.7764712696662 9.46981283 35.54109162548015 139.7764767360794 9.46972449 35.541091627112976 139.77647672316132 3.47272461 35.5410890863794 139.77647125674304 3.47281295</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2400_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2400_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2400_6">
											<gml:posList>35.541091627112976 139.77647672316132 3.47272461 35.54109162548015 139.7764767360794 9.46972449 35.5410404891842 139.77651274298464 9.46891864 35.54104049076874 139.77651273010045 3.47191876 35.541091627112976 139.77647672316132 3.47272461</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_0">
											<gml:posList>35.54104048839679 139.776512749387 12.44891858 35.54103194016309 139.77649442676378 12.44921478 35.54103194089381 139.77649442078308 9.66921484 35.54103085631995 139.77649209606508 9.66925246 35.54103085554495 139.77649210241253 12.6192524 35.54104362961343 139.7765194828593 12.61880988 35.5410436303943 139.77651947652453 9.66880994 35.541040489131305 139.77651274341434 9.66891864 35.54104048839679 139.776512749387 12.44891858</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_2">
											<gml:posList>35.54094359011634 139.7765891996724 12.61726557 35.5409435908508 139.77658919336986 9.66726563 35.5410436303943 139.77651947652453 9.66880994 35.54104362961343 139.7765194828593 12.61880988 35.54094359011634 139.7765891996724 12.61726557</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_3">
											<gml:posList>35.54093081666088 139.77656181300657 9.66770816 35.5409435908508 139.77658919336986 9.66726563 35.54094359011634 139.7765891996724 12.61726557 35.540930815932285 139.77656181932176 12.6177081 35.54093081666088 139.77656181300657 9.66770816</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_4">
											<gml:posList>35.54103085631995 139.77649209606508 9.66925246 35.54093081666088 139.77656181300657 9.66770816 35.540930815932285 139.77656181932176 12.6177081 35.54103085554495 139.77649210241253 12.6192524 35.54103085631995 139.77649209606508 9.66925246</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_5">
											<gml:posList>35.541031940946446 139.77649442035283 9.46921484 35.54103194252293 139.77649440745145 3.47221496 35.541030857948066 139.77649208273127 3.47225258 35.541030856372565 139.77649209563484 9.46925246 35.541031940946446 139.77649442035283 9.46921484</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_6">
											<gml:posList>35.54104363203488 139.77651946321717 3.47181006 35.541043630447284 139.77651947609505 9.46880994 35.54094359090063 139.7765891929426 9.46726563 35.5409435923938 139.77658918013017 3.47026574 35.54104363203488 139.77651946321717 3.47181006</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_7">
											<gml:posList>35.54094359090063 139.7765891929426 9.46726563 35.54093081671035 139.7765618125784 9.46770817 35.54093081819142 139.7765617997403 3.47070828 35.5409435923938 139.77658918013017 3.47026574 35.54094359090063 139.7765891929426 9.46726563</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_8">
											<gml:posList>35.54093081671035 139.7765618125784 9.46770817 35.541030856372565 139.77649209563484 9.46925246 35.541030857948066 139.77649208273127 3.47225258 35.54093081819142 139.7765617997403 3.47070828 35.54093081671035 139.7765618125784 9.46770817</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2399_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2399_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2399_9">
											<gml:posList>35.54104049076874 139.77651273010045 3.47191876 35.5410404891842 139.77651274298464 9.46891864 35.541043630447284 139.77651947609505 9.46880994 35.54104363203488 139.77651946321717 3.47181006 35.54104049076874 139.77651273010045 3.47191876</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2379_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_2">
											<gml:posList>35.5399895250104 139.77704871560493 9.45728514 35.53998952499058 139.77704871601784 9.65728514 35.540966373478824 139.7763538813139 9.67150584 35.541071751855476 139.77627892423467 9.67318749 35.54107175190935 139.7762789237977 9.4731875 35.54106945893421 139.77628055482467 9.4731506 35.54096637352936 139.77635388087933 9.47150585 35.540077235126056 139.776986327996 9.45846108 35.53999505335721 139.7770447833342 9.45735867 35.5399895250104 139.77704871560493 9.45728514</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2379_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_9">
											<gml:posList>35.54107175190935 139.7762789237977 9.4731875 35.541071751855476 139.77627892423467 9.67318749 35.541147814744185 139.77644010124925 9.67056569 35.541147814800404 139.77644010081733 9.4705657 35.541145554314646 139.77643531084433 9.47064297 35.54107175190935 139.7762789237977 9.4731875</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2379_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_10">
											<gml:posList>35.541147814800404 139.77644010081733 9.4705657 35.541147814744185 139.77644010124925 9.67056569 35.54115623360753 139.7764338620741 9.67070574 35.541156233664026 139.77643386164198 9.47070574 35.541147814800404 139.77644010081733 9.4705657</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2379_11">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_11">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_11">
											<gml:posList>35.541165468027444 139.7764560267104 9.47034439 35.541156233664026 139.77643386164198 9.47070574 35.54115623360753 139.7764338620741 9.67070574 35.54116546797068 139.7764560271419 9.67034439 35.541165468027444 139.7764560267104 9.47034439</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2379_12">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_12">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_12">
											<gml:posList>35.54116489164431 139.77645643718992 9.6703351 35.541164891701065 139.77645643675856 9.4703351 35.541165468027444 139.7764560267104 9.47034439 35.54116546797068 139.7764560271419 9.67034439 35.54116489164431 139.77645643718992 9.6703351</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2379_13">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2379_13">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2379_13">
											<gml:posList>35.54007570781268 139.77722977324106 9.45434448 35.54007570779016 139.77722977364823 9.65434447 35.53998952499058 139.77704871601784 9.65728514 35.5399895250104 139.77704871560493 9.45728514 35.5399950550564 139.7770603334051 9.45709475 35.54007570781268 139.77722977324106 9.45434448</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2377_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_0">
											<gml:posList>35.54152953514769 139.77722910425135 11.62841512 35.54152953793409 139.7772290876212 3.46141527 35.5411645376906 139.77645567009307 3.47334735 35.54116453598901 139.77645568303083 9.47034723 35.541164891701065 139.77645643675856 9.4703351 35.54116489164431 139.77645643718992 9.6703351 35.54116453593224 139.77645568346227 9.67034723 35.54116453537332 139.77645568771229 11.64034719 35.54152953514769 139.77722910425135 11.62841512</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2377_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_6">
											<gml:posList>35.540235264368334 139.77757130315788 12.25894653 35.54007451339904 139.7772306266274 11.62432857 35.540439490474704 139.77800409476083 11.612397 35.540235264368334 139.77757130315788 12.25894653</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2377_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_7">
											<gml:posList>35.54152953514769 139.77722910425135 11.62841512 35.540439490474704 139.77800409476083 11.612397 35.54043949186081 139.77800407912213 3.44539712 35.54152953793409 139.7772290876212 3.46141527 35.54152953514769 139.77722910425135 11.62841512</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2377_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_8">
											<gml:posList>35.54043949186081 139.77800407912213 3.44539712 35.540439490474704 139.77800409476083 11.612397 35.54007451339904 139.7772306266274 11.62432857 35.54007451431614 139.7772306099995 3.45732871 35.54043949186081 139.77800407912213 3.44539712</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2377_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2377_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2377_9">
											<gml:posList>35.5411645376906 139.77645567009307 3.47334735 35.54007451431614 139.7772306099995 3.45732871 35.54007451339904 139.7772306266274 11.62432857 35.54116453537332 139.77645568771229 11.64034719 35.54116453593224 139.77645568346227 9.67034723 35.541051395486065 139.77653612054186 9.66854156 35.54007570779016 139.77722977364823 9.65434447 35.54007570781268 139.77722977324106 9.45434448 35.54007726149768 139.77722866868353 9.45436513 35.54105139553922 139.77653612011292 9.46854157 35.541145558183864 139.77646917529958 9.47004204 35.54116453598901 139.77645568303083 9.47034723 35.5411645376906 139.77645567009307 3.47334735</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2444_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2444_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2444_0">
											<gml:posList>35.53999505384806 139.77704927391258 9.45728243 35.53999583433786 139.77704872242296 9.45729275 35.53999583493701 139.7770487100423 3.46029285 35.539995054446486 139.77704926153254 3.46028253 35.53999505384806 139.77704927391258 9.45728243</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2444_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2444_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2444_1">
											<gml:posList>35.53999583433786 139.77704872242296 9.45729275 35.539996614948144 139.77704927365718 9.45728435 35.53999661554801 139.77704926127709 3.46028446 35.53999583493701 139.7770487100423 3.46029285 35.53999583433786 139.77704872242296 9.45729275</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2444_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2444_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2444_2">
											<gml:posList>35.539996614948144 139.77704927365718 9.45728435 35.53999661506861 139.77705037638106 9.45726563 35.53999661566857 139.77705036400195 3.46026573 35.53999661554801 139.77704926127709 3.46028446 35.539996614948144 139.77704927365718 9.45728435</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2444_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2444_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2444_3">
											<gml:posList>35.53999661506861 139.77705037638106 9.45726563 35.53999583457888 139.7770509278707 9.45725531 35.53999583517803 139.77705091549214 3.46025541 35.53999661566857 139.77705036400195 3.46026573 35.53999661506861 139.77705037638106 9.45726563</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2444_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2444_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2444_4">
											<gml:posList>35.53999583457888 139.7770509278707 9.45725531 35.53999505396862 139.77705037663648 9.45726371 35.539995054567044 139.77705036425738 3.46026381 35.53999583517803 139.77705091549214 3.46025541 35.53999583457888 139.7770509278707 9.45725531</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2444_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2444_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2444_5">
											<gml:posList>35.53999505396862 139.77705037663648 9.45726371 35.53999505384806 139.77704927391258 9.45728243 35.539995054446486 139.77704926153254 3.46028253 35.539995054567044 139.77705036425738 3.46026381 35.53999505396862 139.77705037663648 9.45726371</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2443_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2443_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2443_0">
											<gml:posList>35.541069459492284 139.77628537957494 9.47306393 35.54107023997836 139.77628482807054 9.47307642 35.54107024159097 139.77628481497229 3.47607654 35.54106946110418 139.77628536647725 3.47606405 35.541069459492284 139.77628537957494 9.47306393</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2443_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2443_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2443_1">
											<gml:posList>35.54107023997836 139.77628482807054 9.47307642 35.54107102059199 139.77628537930468 9.4730691 35.54107102220533 139.776285366207 3.47606922 35.54107024159097 139.77628481497229 3.47607654 35.54107023997836 139.77628482807054 9.47307642</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2443_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2443_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2443_2">
											<gml:posList>35.54107102059199 139.77628537930468 9.4730691 35.54107102071946 139.77628648204328 9.4730493 35.541071022332886 139.77628646894667 3.47604942 35.54107102220533 139.776285366207 3.47606922 35.54107102059199 139.77628537930468 9.4730691</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2443_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2443_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2443_3">
											<gml:posList>35.54107102071946 139.77628648204328 9.4730493 35.541070240233374 139.77628703354767 9.47303681 35.54107024184599 139.77628702045152 3.47603693 35.541071022332886 139.77628646894667 3.47604942 35.54107102071946 139.77628648204328 9.4730493</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2443_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2443_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2443_4">
											<gml:posList>35.541070240233374 139.77628703354767 9.47303681 35.54106945961975 139.7762864823135 9.47304413 35.54106946123165 139.7762864692169 3.47604425 35.54107024184599 139.77628702045152 3.47603693 35.541070240233374 139.77628703354767 9.47303681</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2443_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2443_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2443_5">
											<gml:posList>35.54106945961975 139.7762864823135 9.47304413 35.541069459492284 139.77628537957494 9.47306393 35.54106946110418 139.77628536647725 3.47606405 35.54106946123165 139.7762864692169 3.47604425 35.54106945961975 139.7762864823135 9.47304413</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2442_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2442_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2442_0">
											<gml:posList>35.541145555613596 139.7764466704887 9.47044123 35.54114633610049 139.77644611898532 9.47045372 35.54114633778485 139.77644610603852 3.47345383 35.541145557297334 139.77644665754246 3.47344135 35.541145555613596 139.7764466704887 9.47044123</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2442_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2442_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2442_1">
											<gml:posList>35.54114633610049 139.77644611898532 9.47045372 35.54114711671338 139.77644667022156 9.47044663 35.54114711839846 139.7764466572753 3.47344675 35.54114633778485 139.77644610603852 3.47345383 35.54114633610049 139.77644611898532 9.47045372</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2442_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2442_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2442_2">
											<gml:posList>35.54114711671338 139.77644667022156 9.47044663 35.54114711683936 139.77644777296118 9.47042705 35.54114711852455 139.77644776001603 3.47342717 35.54114711839846 139.7764466572753 3.47344675 35.54114711671338 139.77644667022156 9.47044663</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2442_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2442_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2442_3">
											<gml:posList>35.54114711683936 139.77644777296118 9.47042705 35.54114633635256 139.77644832446455 9.47041457 35.541146338037024 139.77644831151986 3.47341468 35.54114711852455 139.77644776001603 3.47342717 35.54114711683936 139.77644777296118 9.47042705</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2442_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2442_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2442_4">
											<gml:posList>35.54114633635256 139.77644832446455 9.47041457 35.541145555739675 139.77644777322828 9.47042165 35.54114555742333 139.77644776028305 3.47342177 35.541146338037024 139.77644831151986 3.47341468 35.54114633635256 139.77644832446455 9.47041457</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2442_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2442_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2442_5">
											<gml:posList>35.541145555739675 139.77644777322828 9.47042165 35.541145555613596 139.7764466704887 9.47044123 35.541145557297334 139.77644665754246 3.47344135 35.54114555742333 139.77644776028305 3.47342177 35.541145555739675 139.77644777322828 9.47042165</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2439_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2439_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2439_0">
											<gml:posList>35.54007726077968 139.77722201097356 9.45447664 35.54007804127018 139.77722145948502 9.45448697 35.54007804194694 139.77722144726667 3.45748707 35.540077261455636 139.77722199875575 3.45747675 35.54007726077968 139.77722201097356 9.45447664</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2439_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2439_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2439_1">
											<gml:posList>35.54007804127018 139.77722145948502 9.45448697 35.54007882187966 139.7772220107215 9.45447882 35.54007882255714 139.77722199850356 3.45747892 35.54007804194694 139.77722144726667 3.45748707 35.54007804127018 139.77722145948502 9.45448697</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2439_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2439_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2439_2">
											<gml:posList>35.54007882187966 139.7772220107215 9.45447882 35.54007882199864 139.7772231134465 9.45446034 35.54007882267604 139.77722310122968 3.45746044 35.54007882255714 139.77722199850356 3.45747892 35.54007882187966 139.7772220107215 9.45447882</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2439_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2439_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2439_3">
											<gml:posList>35.54007882199864 139.7772231134465 9.45446034 35.54007804150814 139.77722366493504 9.45445002 35.5400780421848 139.77722365271876 3.45745012 35.54007882267604 139.77722310122968 3.45746044 35.54007882199864 139.7772231134465 9.45446034</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2439_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2439_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2439_4">
											<gml:posList>35.54007804150814 139.77722366493504 9.45445002 35.540077260898656 139.77722311369857 9.45445817 35.540077261574616 139.77722310148175 3.45745827 35.5400780421848 139.77722365271876 3.45745012 35.54007804150814 139.77722366493504 9.45445002</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0876_p2439_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0876_p2439_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0876_p2439_5">
											<gml:posList>35.540077260898656 139.77722311369857 9.45445817 35.54007726077968 139.77722201097356 9.45447664 35.540077261455636 139.77722199875575 3.45747675 35.540077261574616 139.77722310148175 3.45745827 35.540077260898656 139.77722311369857 9.45445817</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:address>
				<core:Address>
					<core:xalAddress>
						<xAL:AddressDetails>
							<xAL:Country>
								<xAL:CountryName>日本</xAL:CountryName>
								<xAL:Locality>
									<xAL:LocalityName Type="Town">東京都大田区羽田空港二丁目</xAL:LocalityName>
								</xAL:Locality>
							</xAL:Country>
						</xAL:AddressDetails>
					</core:xalAddress>
				</core:Address>
			</bldg:address>
			<uro:buildingDetails>
				<uro:BuildingDetails>
					<uro:districtsAndZonesType codeSpace="../../codelists/Common_districtsAndZonesType.xml">11</uro:districtsAndZonesType>
					<uro:prefecture codeSpace="../../codelists/Common_prefecture.xml">13</uro:prefecture>
					<uro:city codeSpace="../../codelists/Common_localPublicAuthorities.xml">13111</uro:city>
				</uro:BuildingDetails>
			</uro:buildingDetails>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">2</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key2.xml">2</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">106</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key106.xml">20</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
		</bldg:Building>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<bldg:Building gml:id="BLD_8a3e0e2c-9a3e-4b17-bb60-6a744badeb13">
			<gen:stringAttribute name="建物ID">
				<gen:value>13111-bldg-54</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="大字・町コード">
				<gen:value>42</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="町・丁目コード">
				<gen:value>2</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="13_区市町村コード_大字・町コード_町・丁目コード">
				<gen:value>13111042002</gen:value>
			</gen:stringAttribute>
			<gen:genericAttributeSet name="多摩水系多摩川、浅川、大栗川洪水浸水想定区域（想定最大規模）">
				<gen:stringAttribute name="規模">
					<gen:value>L2</gen:value>
				</gen:stringAttribute>
				<gen:stringAttribute name="浸水ランク">
					<gen:value>2</gen:value>
				</gen:stringAttribute>
				<gen:measureAttribute name="浸水深">
					<gen:value uom="m">0.990</gen:value>
				</gen:measureAttribute>
				<gen:measureAttribute name="継続時間">
					<gen:value uom="hour">0.68</gen:value>
				</gen:measureAttribute>
			</gen:genericAttributeSet>
			<bldg:measuredHeight uom="m">3.9</bldg:measuredHeight>
			<bldg:lod0RoofEdge>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>35.53954520137065 139.7765597856091 6.514 35.53957212141832 139.7765405310804 6.514 35.53948882530161 139.77636619287276 6.514 35.53944012775923 139.77640099837194 6.514 35.53952341031053 139.77657534754815 6.514 35.53954520137065 139.7765597856091 6.514</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</bldg:lod0RoofEdge>
			<bldg:lod1Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53954520137065 139.7765597856091 2.817 35.53952341031053 139.77657534754815 2.817 35.53944012775923 139.77640099837194 2.817 35.53948882530161 139.77636619287276 2.817 35.53957212141832 139.7765405310804 2.817 35.53954520137065 139.7765597856091 2.817</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53954520137065 139.7765597856091 2.817 35.53957212141832 139.7765405310804 2.817 35.53957212141832 139.7765405310804 6.514 35.53954520137065 139.7765597856091 6.514 35.53954520137065 139.7765597856091 2.817</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53957212141832 139.7765405310804 2.817 35.53948882530161 139.77636619287276 2.817 35.53948882530161 139.77636619287276 6.514 35.53957212141832 139.7765405310804 6.514 35.53957212141832 139.7765405310804 2.817</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53948882530161 139.77636619287276 2.817 35.53944012775923 139.77640099837194 2.817 35.53944012775923 139.77640099837194 6.514 35.53948882530161 139.77636619287276 6.514 35.53948882530161 139.77636619287276 2.817</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53944012775923 139.77640099837194 2.817 35.53952341031053 139.77657534754815 2.817 35.53952341031053 139.77657534754815 6.514 35.53944012775923 139.77640099837194 6.514 35.53944012775923 139.77640099837194 2.817</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53952341031053 139.77657534754815 2.817 35.53954520137065 139.7765597856091 2.817 35.53954520137065 139.7765597856091 6.514 35.53952341031053 139.77657534754815 6.514 35.53952341031053 139.77657534754815 2.817</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.53954520137065 139.7765597856091 6.514 35.53957212141832 139.7765405310804 6.514 35.53948882530161 139.77636619287276 6.514 35.53944012775923 139.77640099837194 6.514 35.53952341031053 139.77657534754815 6.514 35.53954520137065 139.7765597856091 6.514</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod1Solid>
			<bldg:lod2Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_b_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0279_p2471_9"/>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod2Solid>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_4cc1a87b-838b-4528-89ec-6065d8442251">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_b_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_b_0">
											<gml:posList>35.53952341010228 139.77657534079654 2.93710889 35.53944012733283 139.77640099466805 2.94017523 35.53948882533167 139.77636619277283 2.94080567 35.53957212185765 139.77654052799713 2.93773957 35.53952341010228 139.77657534079654 2.93710889</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0279_p2471_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_0">
											<gml:posList>35.53952341002441 139.7765753473043 5.98010883 35.53946214193544 139.77638588638493 6.7734484 35.539440127294824 139.7764010012588 5.98317517 35.53952341002441 139.7765753473043 5.98010883</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0279_p2471_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_1">
											<gml:posList>35.539545201586286 139.77655978546414 6.77039002 35.53946214193544 139.77638588638493 6.7734484 35.53952341002441 139.7765753473043 5.98010883 35.539545201586286 139.77655978546414 6.77039002</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0279_p2471_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_2">
											<gml:posList>35.53946214193544 139.77638588638493 6.7734484 35.539545201586286 139.77655978546414 6.77039002 35.539488825270425 139.77636619938025 5.98380561 35.53946214193544 139.77638588638493 6.7734484</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0279_p2471_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_3">
											<gml:posList>35.539488825270425 139.77636619938025 5.98380561 35.539545201586286 139.77655978546414 6.77039002 35.539572121756436 139.77654053452144 5.98073951 35.539488825270425 139.77636619938025 5.98380561</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0279_p2471_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_4">
											<gml:posList>35.539545201586286 139.77655978546414 6.77039002 35.53952341002441 139.7765753473043 5.98010883 35.539572121756436 139.77654053452144 5.98073951 35.539545201586286 139.77655978546414 6.77039002</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0279_p2471_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_5">
											<gml:posList>35.53946214193544 139.77638588638493 6.7734484 35.539488825270425 139.77636619938025 5.98380561 35.539440127294824 139.7764010012588 5.98317517 35.53946214193544 139.77638588638493 6.7734484</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0279_p2471_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_6">
											<gml:posList>35.539572121756436 139.77654053452144 5.98073951 35.53952341002441 139.7765753473043 5.98010883 35.53952341010228 139.77657534079654 2.93710889 35.53957212185765 139.77654052799713 2.93773957 35.539572121756436 139.77654053452144 5.98073951</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0279_p2471_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_7">
											<gml:posList>35.53952341002441 139.7765753473043 5.98010883 35.539440127294824 139.7764010012588 5.98317517 35.53944012733283 139.77640099466805 2.94017523 35.53952341010228 139.77657534079654 2.93710889 35.53952341002441 139.7765753473043 5.98010883</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0279_p2471_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_8">
											<gml:posList>35.539440127294824 139.7764010012588 5.98317517 35.539488825270425 139.77636619938025 5.98380561 35.53948882533167 139.77636619277283 2.94080567 35.53944012733283 139.77640099466805 2.94017523 35.539440127294824 139.7764010012588 5.98317517</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0279_p2471_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0279_p2471_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0279_p2471_9">
											<gml:posList>35.539488825270425 139.77636619938025 5.98380561 35.539572121756436 139.77654053452144 5.98073951 35.53957212185765 139.77654052799713 2.93773957 35.53948882533167 139.77636619277283 2.94080567 35.539488825270425 139.77636619938025 5.98380561</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:address>
				<core:Address>
					<core:xalAddress>
						<xAL:AddressDetails>
							<xAL:Country>
								<xAL:CountryName>日本</xAL:CountryName>
								<xAL:Locality>
									<xAL:LocalityName Type="Town">東京都大田区羽田空港二丁目</xAL:LocalityName>
								</xAL:Locality>
							</xAL:Country>
						</xAL:AddressDetails>
					</core:xalAddress>
				</core:Address>
			</bldg:address>
			<uro:buildingDetails>
				<uro:BuildingDetails>
					<uro:buildingRoofEdgeArea uom="m2">115.77893</uro:buildingRoofEdgeArea>
					<uro:districtsAndZonesType codeSpace="../../codelists/Common_districtsAndZonesType.xml">11</uro:districtsAndZonesType>
					<uro:prefecture codeSpace="../../codelists/Common_prefecture.xml">13</uro:prefecture>
					<uro:city codeSpace="../../codelists/Common_localPublicAuthorities.xml">13111</uro:city>
					<uro:surveyYear>2016</uro:surveyYear>
				</uro:BuildingDetails>
			</uro:buildingDetails>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">2</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key2.xml">2</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">106</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key106.xml">20</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
		</bldg:Building>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<bldg:Building gml:id="BLD_d818b853-c26d-4e90-b570-374ee4cc10a0">
			<gen:stringAttribute name="建物ID">
				<gen:value>13111-bldg-605</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="大字・町コード">
				<gen:value>42</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="町・丁目コード">
				<gen:value>2</gen:value>
			</gen:stringAttribute>
			<gen:stringAttribute name="13_区市町村コード_大字・町コード_町・丁目コード">
				<gen:value>13111042002</gen:value>
			</gen:stringAttribute>
			<bldg:measuredHeight uom="m">11.6</bldg:measuredHeight>
			<bldg:lod0RoofEdge>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>35.54119340304891 139.77644020977826 12.479000000000001 35.541174841111065 139.7764526112212 12.479000000000001 35.54120271160703 139.77651259883396 12.479000000000001 35.541201125687515 139.77651372706742 12.479000000000001 35.54136836247011 139.7768659876598 12.479000000000001 35.541528362851494 139.77720299011645 12.479000000000001 35.54192121627083 139.77692562451375 12.479000000000001 35.5415946655996 139.77623888870215 12.479000000000001 35.541614085312055 139.7762238915127 12.479000000000001 35.54159183294317 139.77617698069997 12.479000000000001 35.541583801252244 139.77616117142307 12.479000000000001 35.54156226762571 139.77617583926468 12.479000000000001 35.5411931558161 139.77643967727528 12.479000000000001 35.54119340304891 139.77644020977826 12.479000000000001</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</bldg:lod0RoofEdge>
			<bldg:lod1Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54119340304891 139.77644020977826 3.505 35.5411931558161 139.77643967727528 3.505 35.54156226762571 139.77617583926468 3.505 35.541583801252244 139.77616117142307 3.505 35.54159183294317 139.77617698069997 3.505 35.541614085312055 139.7762238915127 3.505 35.5415946655996 139.77623888870215 3.505 35.54192121627083 139.77692562451375 3.505 35.541528362851494 139.77720299011645 3.505 35.54136836247011 139.7768659876598 3.505 35.541201125687515 139.77651372706742 3.505 35.54120271160703 139.77651259883396 3.505 35.541174841111065 139.7764526112212 3.505 35.54119340304891 139.77644020977826 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54119340304891 139.77644020977826 3.505 35.541174841111065 139.7764526112212 3.505 35.541174841111065 139.7764526112212 12.479 35.54119340304891 139.77644020977826 12.479 35.54119340304891 139.77644020977826 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541174841111065 139.7764526112212 3.505 35.54120271160703 139.77651259883396 3.505 35.54120271160703 139.77651259883396 12.479 35.541174841111065 139.7764526112212 12.479 35.541174841111065 139.7764526112212 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54120271160703 139.77651259883396 3.505 35.541201125687515 139.77651372706742 3.505 35.541201125687515 139.77651372706742 12.479 35.54120271160703 139.77651259883396 12.479 35.54120271160703 139.77651259883396 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541201125687515 139.77651372706742 3.505 35.54136836247011 139.7768659876598 3.505 35.54136836247011 139.7768659876598 12.479 35.541201125687515 139.77651372706742 12.479 35.541201125687515 139.77651372706742 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54136836247011 139.7768659876598 3.505 35.541528362851494 139.77720299011645 3.505 35.541528362851494 139.77720299011645 12.479 35.54136836247011 139.7768659876598 12.479 35.54136836247011 139.7768659876598 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541528362851494 139.77720299011645 3.505 35.54192121627083 139.77692562451375 3.505 35.54192121627083 139.77692562451375 12.479 35.541528362851494 139.77720299011645 12.479 35.541528362851494 139.77720299011645 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54192121627083 139.77692562451375 3.505 35.5415946655996 139.77623888870215 3.505 35.5415946655996 139.77623888870215 12.479 35.54192121627083 139.77692562451375 12.479 35.54192121627083 139.77692562451375 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.5415946655996 139.77623888870215 3.505 35.541614085312055 139.7762238915127 3.505 35.541614085312055 139.7762238915127 12.479 35.5415946655996 139.77623888870215 12.479 35.5415946655996 139.77623888870215 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541614085312055 139.7762238915127 3.505 35.54159183294317 139.77617698069997 3.505 35.54159183294317 139.77617698069997 12.479 35.541614085312055 139.7762238915127 12.479 35.541614085312055 139.7762238915127 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54159183294317 139.77617698069997 3.505 35.541583801252244 139.77616117142307 3.505 35.541583801252244 139.77616117142307 12.479 35.54159183294317 139.77617698069997 12.479 35.54159183294317 139.77617698069997 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541583801252244 139.77616117142307 3.505 35.54156226762571 139.77617583926468 3.505 35.54156226762571 139.77617583926468 12.479 35.541583801252244 139.77616117142307 12.479 35.541583801252244 139.77616117142307 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54156226762571 139.77617583926468 3.505 35.5411931558161 139.77643967727528 3.505 35.5411931558161 139.77643967727528 12.479 35.54156226762571 139.77617583926468 12.479 35.54156226762571 139.77617583926468 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.5411931558161 139.77643967727528 3.505 35.54119340304891 139.77644020977826 3.505 35.54119340304891 139.77644020977826 12.479 35.5411931558161 139.77643967727528 12.479 35.5411931558161 139.77643967727528 3.505</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54119340304891 139.77644020977826 12.479 35.541174841111065 139.7764526112212 12.479 35.54120271160703 139.77651259883396 12.479 35.541201125687515 139.77651372706742 12.479 35.54136836247011 139.7768659876598 12.479 35.541528362851494 139.77720299011645 12.479 35.54192121627083 139.77692562451375 12.479 35.5415946655996 139.77623888870215 12.479 35.541614085312055 139.7762238915127 12.479 35.54159183294317 139.77617698069997 12.479 35.541583801252244 139.77616117142307 12.479 35.54156226762571 139.77617583926468 12.479 35.5411931558161 139.77643967727528 12.479 35.54119340304891 139.77644020977826 12.479</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod1Solid>
			<bldg:lod2Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_b_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2396_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2391_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2395_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2392_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2394_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2393_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2389_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2388_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2387_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2396_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2396_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2396_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2396_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2390_11"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2391_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2391_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2391_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2391_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2395_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2395_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2395_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2395_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2392_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2392_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2392_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2392_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2394_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2394_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2394_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2394_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2393_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2393_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2393_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2393_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2389_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2388_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2388_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2388_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2387_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2387_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2387_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2387_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2382_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_9"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_10"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0878_p2380_11"/>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod2Solid>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_4965ba38-30d8-4f51-96e7-155f567afc78">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_b_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_b_0">
											<gml:posList>35.541201126958725 139.77651371804367 3.62944879 35.541202723055754 139.7765125912581 3.62947439 35.54117484305853 139.77645259898878 3.63043794 35.54119340867157 139.77644020566427 3.63072351 35.54119315629109 139.77643967145764 3.6307321 35.541562210269795 139.77617571577326 3.63690662 35.541562325766385 139.77617596202208 3.63690265 35.54158380147117 139.77616117114775 3.63726253 35.54159183251357 139.77617698090253 3.63701085 35.54161408652667 139.7762238836845 3.63625995 35.541594660266796 139.77623887810077 3.63590521 35.54192121842839 139.77692561563276 3.6253497 35.541528365931285 139.77720297306954 3.61884808 35.541201126958725 139.77651371804367 3.62944879</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2396_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2396_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2396_0">
											<gml:posList>35.54157144977565 139.77621378571345 12.28125808 35.541558370168815 139.77618630360928 12.28169886 35.5415392845142 139.77619931187448 12.28138281 35.54155236334148 139.77622679169488 12.28094204 35.54157144977565 139.77621378571345 12.28125808</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2390_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_0">
											<gml:posList>35.541545149792135 139.77624457093663 9.43559096 35.54155437218636 139.77626434996066 9.43527403 35.54158517073534 139.77624260708996 9.43579717 35.54157145076504 139.77621377946784 9.43625813 35.54155236432231 139.7762267854551 9.4359421 35.54153928548916 139.77619930562247 9.43638287 35.541558371152334 139.77618629735153 9.43669892 35.5415555929593 139.7761804599779 9.43679271 35.54152534427828 139.7762020946161 9.43627385 35.541535340830706 139.77622353393036 9.43592879 35.541545149792135 139.77624457093663 9.43559096</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2390_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_1">
											<gml:posList>35.54157145076504 139.77621377946784 9.43625813 35.54158517073534 139.77624260708996 9.43579717 35.54158914308304 139.7762398027278 9.43586483 35.54155987387067 139.7761773981579 9.43686633 35.5415555929593 139.7761804599779 9.43679271 35.541558371152334 139.77618629735153 9.43669892 35.54157145076504 139.77621377946784 9.43625813</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2390_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_2">
											<gml:posList>35.5415117075842 139.77629446988408 9.43455337 35.54155437218636 139.77626434996066 9.43527403 35.541545149792135 139.77624457093663 9.43559096 35.541507924931864 139.77626981085092 9.43498062 35.54149811591885 139.77624877391355 9.43531846 35.541535340830706 139.77622353393036 9.43592879 35.54152534427828 139.7762020946161 9.43627385 35.541482815640165 139.77623251216465 9.43554837 35.54149168507853 139.77625153233944 9.43524222 35.54150061995839 139.77627069286171 9.43493444 35.5415117075842 139.77629446988408 9.43455337</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2390_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_3">
											<gml:posList>35.5414903172668 139.7763095707895 9.43419383 35.5415117075842 139.77629446988408 9.43455337 35.54150061995839 139.77627069286171 9.43493444 35.5414843264403 139.77628180549166 9.43466746 35.541475380987514 139.77626265149618 9.43497509 35.54149168507853 139.77625153233944 9.43524222 35.541482815640165 139.77623251216465 9.43554837 35.54146149417574 139.77624776179644 9.43518642 35.54147122845741 139.77626863623297 9.43485046 35.541483994511445 139.77629601209728 9.43441101 35.5414903172668 139.7763095707895 9.43419383</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2390_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_4">
											<gml:posList>35.54146149417574 139.77624776179644 9.43518642 35.541331316718825 139.77634086768524 9.43300214 35.54134391326734 139.7763654520402 9.43261096 35.54134969206333 139.77637673037123 9.43243187 35.541353018440226 139.77638322237934 9.43232888 35.54135685568449 139.77638054265654 9.43239149 35.541362642000614 139.77639183962944 9.43221246 35.54136560091815 139.77639761650371 9.432121 35.5414903172668 139.7763095707895 9.43419383 35.541483994511445 139.77629601209728 9.43441101 35.54141550510651 139.77634298325538 9.4332924 35.54140273974018 139.77631560977125 9.43373186 35.54147122845741 139.77626863623297 9.43485046 35.54146149417574 139.77624776179644 9.43518642</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2390_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_5">
											<gml:posList>35.541261817385525 139.7764708840823 9.43042665 35.54136560091815 139.77639761650371 9.432121 35.541362642000614 139.77639183962944 9.43221246 35.541273341280906 139.7764542126057 9.43076483 35.541267544525056 139.77644291263871 9.43094397 35.541353018440226 139.77638322237934 9.43232888 35.54134969206333 139.77637673037123 9.43243187 35.54126334922136 139.77643821357842 9.43101195 35.54125756299815 139.7764269165428 9.43119142 35.54134391326734 139.7763654520402 9.43261096 35.541331316718825 139.77634086768524 9.43300214 35.54119315461932 139.776439683985 9.43073199 35.54119340699945 139.77644021819108 9.4307234 35.54123474290711 139.77641262471718 9.43136232 35.541261817385525 139.7764708840823 9.43042665</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2390_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_10">
											<gml:posList>35.5415622075687 139.77617573294293 11.43690646 35.54155987317886 139.7761774025598 11.43686629 35.54158914238193 139.77623980711016 11.43586479 35.54159148438464 139.77623815372405 11.43590469 35.54158001914165 139.77621370879032 11.4362962 35.54157927830208 139.7762142347496 11.43628352 35.54156940594443 139.77619353482035 11.43661516 35.54156103020435 139.77617686961784 11.43688087 35.541562323065115 139.77617597919138 11.43690249 35.5415622075687 139.77617573294293 11.43690646</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2391_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2391_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2391_0">
											<gml:posList>35.54154514828541 139.7762445805418 13.82059087 35.54153533933075 139.7762235435499 13.8209287 35.541498114444565 139.7762487835157 13.82031837 35.54150792345082 139.77626982043864 13.81998053 35.54154514828541 139.7762445805418 13.82059087</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2395_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2395_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2395_0">
											<gml:posList>35.54148432497547 139.77628181507123 13.81966737 35.54150061848229 139.7762707024489 13.81993435 35.54149168360867 139.77625154193976 13.82024213 35.54147537952892 139.77626266108888 13.819975 35.54148432497547 139.77628181507123 13.81966737</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2392_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2392_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2392_0">
											<gml:posList>35.541471226998276 139.77626864584332 13.82985037 35.54140273832852 139.77631561934922 13.82873177 35.54141550368592 139.7763429928146 13.82829232 35.541483993043556 139.77629602168886 13.82941092 35.541471226998276 139.77626864584332 13.82985037</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2394_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2394_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2394_0">
											<gml:posList>35.54136264088579 139.77639184730202 12.97221239 35.541356854572996 139.7763805503353 12.97239142 35.5412675434633 139.7764429202829 12.9709439 35.54127334021591 139.7764542202436 12.97076476 35.54136264088579 139.77639184730202 12.97221239</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2393_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2393_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2393_0">
											<gml:posList>35.54134969095735 139.77637673804125 12.9674318 35.54134391216459 139.77636545971652 12.96761089 35.541257561943425 139.7764269241851 12.96619135 35.54126334816338 139.77643822121445 12.96601188 35.54134969095735 139.77637673804125 12.9674318</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2389_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2389_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2389_0">
											<gml:posList>35.541591484514505 139.77623815291327 11.0659047 35.541593596169655 139.77623666214578 11.06594069 35.54159465765587 139.77623889438246 11.06590506 35.54161408389313 139.77622389998362 11.0662598 35.541603919052505 139.77620247645532 11.06660231 35.541582593236484 139.7762191959039 11.06620823 35.541591484514505 139.77623815291327 11.0659047</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2388_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2388_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2388_0">
											<gml:posList>35.54160391901898 139.77620247666414 11.1616679 35.541591829299655 139.77617700105887 12.79466987 35.54156940548304 139.77619353774003 12.76470467 35.54157927830208 139.7762142347496 11.43628352 35.54158001914165 139.77621370879032 11.4362962 35.54158259322974 139.77621919594645 11.08558366 35.54160391901898 139.77620247666414 11.1616679</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2387_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2387_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2387_1">
											<gml:posList>35.54158379826629 139.7761611913429 12.80226234 35.541561029731895 139.77617687262224 12.80188084 35.541569405470185 139.77619353782123 12.80161513 35.54159182929704 139.77617700107498 12.80201066 35.54158379826629 139.7761611913429 12.80226234</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2382_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_4">
											<gml:posList>35.54117916888155 139.77645413053614 15.0854262 35.54123353107959 139.77641784147195 15.08626521 35.541261436625426 139.77647788915175 15.08530125 35.541265765709106 139.77647939599214 15.08529052 35.54123474124008 139.77641263695517 15.08636221 35.54117483978965 139.77645262370697 15.08543772 35.54120586438744 139.77651938254502 15.08436587 35.541207074544765 139.77651417803946 15.08446212 35.54117916888155 139.77645413053614 15.0854262</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2382_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_5">
											<gml:posList>35.541179169453606 139.77645412622093 13.08542624 35.541207075125676 139.77651417374312 13.08446216 35.54126143722336 139.77647788484393 13.08530129 35.54123353166877 139.7764178371454 13.08626525 35.541179169453606 139.77645412622093 13.08542624</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2382_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_10">
											<gml:posList>35.541261436625426 139.77647788915175 15.08530125 35.541207074544765 139.77651417803946 15.08446212 35.54120586438744 139.77651938254502 15.08436587 35.541265765709106 139.77647939599214 15.08529052 35.541261436625426 139.77647788915175 15.08530125</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2380_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_0">
											<gml:posList>35.541202720581325 139.7765126096059 12.16947422 35.54120112448638 139.77651373638992 12.16944862 35.541203385514535 139.7765140404247 12.17379143 35.541202720581325 139.7765126096059 12.16947422</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2380_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_1">
											<gml:posList>35.54136836284369 139.77686598811104 13.23892866 35.541761156738005 139.77658904303513 13.24542141 35.54121210265773 139.77651521259696 12.19053474 35.541205865227504 139.7765193763294 12.18990371 35.541203385514535 139.7765140404247 12.17379143 35.54120112448638 139.77651373638992 12.16944862 35.54136836284369 139.77686598811104 13.23892866</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2380_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_2">
											<gml:posList>35.541761156738005 139.77658904303513 13.24542141 35.54136836284369 139.77686598811104 13.23892866 35.541921214988626 139.7769256334283 12.16534953 35.541761156738005 139.77658904303513 13.24542141</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2380_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_3">
											<gml:posList>35.54126576657494 139.77647938976978 12.19607298 35.54121210265773 139.77651521259696 12.19053474 35.541761156738005 139.77658904303513 13.24542141 35.54159359577978 139.77623666457848 12.17594067 35.54126181656615 139.77647088998697 12.1704266 35.54126576657494 139.77647938976978 12.19607298</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0878_p2380_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_4">
											<gml:posList>35.541921214988626 139.7769256334283 12.16534953 35.54136836284369 139.77686598811104 13.23892866 35.541528363019246 139.7772029904941 12.15884792 35.541921214988626 139.7769256334283 12.16534953</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2396_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2396_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2396_1">
											<gml:posList>35.54157145076504 139.77621377946784 9.43625813 35.54157144977565 139.77621378571345 12.28125808 35.54155236334148 139.77622679169488 12.28094204 35.54155236432231 139.7762267854551 9.4359421 35.54157145076504 139.77621377946784 9.43625813</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2396_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2396_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2396_2">
											<gml:posList>35.54155236432231 139.7762267854551 9.4359421 35.54155236334148 139.77622679169488 12.28094204 35.5415392845142 139.77619931187448 12.28138281 35.54153928548916 139.77619930562247 9.43638287 35.54155236432231 139.7762267854551 9.4359421</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2396_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2396_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2396_3">
											<gml:posList>35.54153928548916 139.77619930562247 9.43638287 35.5415392845142 139.77619931187448 12.28138281 35.541558370168815 139.77618630360928 12.28169886 35.541558371152334 139.77618629735153 9.43669892 35.54153928548916 139.77619930562247 9.43638287</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2396_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2396_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2396_4">
											<gml:posList>35.541558371152334 139.77618629735153 9.43669892 35.541558370168815 139.77618630360928 12.28169886 35.54157144977565 139.77621378571345 12.28125808 35.54157145076504 139.77621377946784 9.43625813 35.541558371152334 139.77618629735153 9.43669892</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2390_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_6">
											<gml:posList>35.54119340867157 139.77644020566427 3.63072351 35.54119340699945 139.77644021819108 9.4307234 35.54119315461932 139.776439683985 9.43073199 35.54119315629109 139.77643967145764 3.6307321 35.54119340867157 139.77644020566427 3.63072351</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2390_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_7">
											<gml:posList>35.541562210269795 139.77617571577326 3.63690662 35.54119315629109 139.77643967145764 3.6307321 35.54119315461932 139.776439683985 9.43073199 35.541331316718825 139.77634086768524 9.43300214 35.54146149417574 139.77624776179644 9.43518642 35.541482815640165 139.77623251216465 9.43554837 35.54152534427828 139.7762020946161 9.43627385 35.5415555929593 139.7761804599779 9.43679271 35.54155987387067 139.7761773981579 9.43686633 35.54155987317886 139.7761774025598 11.43686629 35.5415622075687 139.77617573294293 11.43690646 35.541562210269795 139.77617571577326 3.63690662</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2390_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_8">
											<gml:posList>35.541562210269795 139.77617571577326 3.63690662 35.5415622075687 139.77617573294293 11.43690646 35.541562323065115 139.77617597919138 11.43690249 35.541562325766385 139.77617596202208 3.63690265 35.541562210269795 139.77617571577326 3.63690662</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2390_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_9">
											<gml:posList>35.54155987387067 139.7761773981579 9.43686633 35.54158914308304 139.7762398027278 9.43586483 35.54158914238193 139.77623980711016 11.43586479 35.54155987317886 139.7761774025598 11.43686629 35.54155987387067 139.7761773981579 9.43686633</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2390_11">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2390_11">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2390_11">
											<gml:posList>35.54158259322974 139.77621919594645 11.08558366 35.54158001914165 139.77621370879032 11.4362962 35.54159148438464 139.77623815372405 11.43590469 35.541591484514505 139.77623815291327 11.0659047 35.541582593236484 139.7762191959039 11.06620823 35.54158259322974 139.77621919594645 11.08558366</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2391_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2391_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2391_1">
											<gml:posList>35.541545149792135 139.77624457093663 9.43559096 35.54154514828541 139.7762445805418 13.82059087 35.54150792345082 139.77626982043864 13.81998053 35.541507924931864 139.77626981085092 9.43498062 35.541545149792135 139.77624457093663 9.43559096</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2391_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2391_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2391_2">
											<gml:posList>35.541507924931864 139.77626981085092 9.43498062 35.54150792345082 139.77626982043864 13.81998053 35.541498114444565 139.7762487835157 13.82031837 35.54149811591885 139.77624877391355 9.43531846 35.541507924931864 139.77626981085092 9.43498062</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2391_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2391_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2391_3">
											<gml:posList>35.54149811591885 139.77624877391355 9.43531846 35.541498114444565 139.7762487835157 13.82031837 35.54153533933075 139.7762235435499 13.8209287 35.541535340830706 139.77622353393036 9.43592879 35.54149811591885 139.77624877391355 9.43531846</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2391_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2391_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2391_4">
											<gml:posList>35.541535340830706 139.77622353393036 9.43592879 35.54153533933075 139.7762235435499 13.8209287 35.54154514828541 139.7762445805418 13.82059087 35.541545149792135 139.77624457093663 9.43559096 35.541535340830706 139.77622353393036 9.43592879</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2395_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2395_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2395_1">
											<gml:posList>35.54150061995839 139.77627069286171 9.43493444 35.54150061848229 139.7762707024489 13.81993435 35.54148432497547 139.77628181507123 13.81966737 35.5414843264403 139.77628180549166 9.43466746 35.54150061995839 139.77627069286171 9.43493444</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2395_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2395_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2395_2">
											<gml:posList>35.5414843264403 139.77628180549166 9.43466746 35.54148432497547 139.77628181507123 13.81966737 35.54147537952892 139.77626266108888 13.819975 35.541475380987514 139.77626265149618 9.43497509 35.5414843264403 139.77628180549166 9.43466746</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2395_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2395_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2395_3">
											<gml:posList>35.541475380987514 139.77626265149618 9.43497509 35.54147537952892 139.77626266108888 13.819975 35.54149168360867 139.77625154193976 13.82024213 35.54149168507853 139.77625153233944 9.43524222 35.541475380987514 139.77626265149618 9.43497509</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2395_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2395_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2395_4">
											<gml:posList>35.54150061995839 139.77627069286171 9.43493444 35.54149168507853 139.77625153233944 9.43524222 35.54149168360867 139.77625154193976 13.82024213 35.54150061848229 139.7762707024489 13.81993435 35.54150061995839 139.77627069286171 9.43493444</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2392_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2392_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2392_1">
											<gml:posList>35.54141550510651 139.77634298325538 9.4332924 35.541483994511445 139.77629601209728 9.43441101 35.541483993043556 139.77629602168886 13.82941092 35.54141550368592 139.7763429928146 13.82829232 35.54141550510651 139.77634298325538 9.4332924</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2392_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2392_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2392_2">
											<gml:posList>35.54140273832852 139.77631561934922 13.82873177 35.54140273974018 139.77631560977125 9.43373186 35.54141550510651 139.77634298325538 9.4332924 35.54141550368592 139.7763429928146 13.82829232 35.54140273832852 139.77631561934922 13.82873177</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2392_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2392_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2392_3">
											<gml:posList>35.541471226998276 139.77626864584332 13.82985037 35.54147122845741 139.77626863623297 9.43485046 35.54140273974018 139.77631560977125 9.43373186 35.54140273832852 139.77631561934922 13.82873177 35.541471226998276 139.77626864584332 13.82985037</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2392_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2392_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2392_4">
											<gml:posList>35.541483993043556 139.77629602168886 13.82941092 35.541483994511445 139.77629601209728 9.43441101 35.54147122845741 139.77626863623297 9.43485046 35.541471226998276 139.77626864584332 13.82985037 35.541483993043556 139.77629602168886 13.82941092</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2394_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2394_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2394_1">
											<gml:posList>35.541273341280906 139.7764542126057 9.43076483 35.541362642000614 139.77639183962944 9.43221246 35.54136264088579 139.77639184730202 12.97221239 35.54127334021591 139.7764542202436 12.97076476 35.541273341280906 139.7764542126057 9.43076483</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2394_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2394_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2394_2">
											<gml:posList>35.541273341280906 139.7764542126057 9.43076483 35.54127334021591 139.7764542202436 12.97076476 35.5412675434633 139.7764429202829 12.9709439 35.541267544525056 139.77644291263871 9.43094397 35.541273341280906 139.7764542126057 9.43076483</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2394_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2394_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2394_3">
											<gml:posList>35.541353018440226 139.77638322237934 9.43232888 35.541267544525056 139.77644291263871 9.43094397 35.5412675434633 139.7764429202829 12.9709439 35.541356854572996 139.7763805503353 12.97239142 35.54135685568449 139.77638054265654 9.43239149 35.541353018440226 139.77638322237934 9.43232888</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2394_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2394_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2394_4">
											<gml:posList>35.54135685568449 139.77638054265654 9.43239149 35.541356854572996 139.7763805503353 12.97239142 35.54136264088579 139.77639184730202 12.97221239 35.541362642000614 139.77639183962944 9.43221246 35.54135685568449 139.77638054265654 9.43239149</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2393_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2393_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2393_1">
											<gml:posList>35.54134969206333 139.77637673037123 9.43243187 35.54134969095735 139.77637673804125 12.9674318 35.54126334816338 139.77643822121445 12.96601188 35.54126334922136 139.77643821357842 9.43101195 35.54134969206333 139.77637673037123 9.43243187</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2393_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2393_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2393_2">
											<gml:posList>35.54126334922136 139.77643821357842 9.43101195 35.54126334816338 139.77643822121445 12.96601188 35.541257561943425 139.7764269241851 12.96619135 35.54125756299815 139.7764269165428 9.43119142 35.54126334922136 139.77643821357842 9.43101195</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2393_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2393_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2393_3">
											<gml:posList>35.54125756299815 139.7764269165428 9.43119142 35.541257561943425 139.7764269241851 12.96619135 35.54134391216459 139.77636545971652 12.96761089 35.54134391326734 139.7763654520402 9.43261096 35.54125756299815 139.7764269165428 9.43119142</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2393_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2393_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2393_4">
											<gml:posList>35.54134391326734 139.7763654520402 9.43261096 35.54134391216459 139.77636545971652 12.96761089 35.54134969095735 139.77637673804125 12.9674318 35.54134969206333 139.77637673037123 9.43243187 35.54134391326734 139.7763654520402 9.43261096</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2389_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2389_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2389_1">
											<gml:posList>35.54159465765587 139.77623889438246 11.06590506 35.541594660266796 139.77623887810077 3.63590521 35.54161408652667 139.7762238836845 3.63625995 35.54161408389313 139.77622389998362 11.0662598 35.54159465765587 139.77623889438246 11.06590506</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2388_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2388_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2388_1">
											<gml:posList>35.54161408389313 139.77622389998362 11.0662598 35.54161408652667 139.7762238836845 3.63625995 35.54159183251357 139.77617698090253 3.63701085 35.541591829299655 139.77617700105887 12.79466987 35.54160391901898 139.77620247666414 11.1616679 35.541603919052505 139.77620247645532 11.06660231 35.54161408389313 139.77622389998362 11.0662598</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2388_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2388_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2388_2">
											<gml:posList>35.541582593236484 139.7762191959039 11.06620823 35.541603919052505 139.77620247645532 11.06660231 35.54160391901898 139.77620247666414 11.1616679 35.54158259322974 139.77621919594645 11.08558366 35.541582593236484 139.7762191959039 11.06620823</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2388_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2388_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2388_3">
											<gml:posList>35.54156940548304 139.77619353774003 12.76470467 35.54156940594443 139.77619353482035 11.43661516 35.54157927830208 139.7762142347496 11.43628352 35.54156940548304 139.77619353774003 12.76470467</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2387_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2387_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2387_0">
											<gml:posList>35.541562325766385 139.77617596202208 3.63690265 35.541562323065115 139.77617597919138 11.43690249 35.54156103020435 139.77617686961784 11.43688087 35.541561029731895 139.77617687262224 12.80188084 35.54158379826629 139.7761611913429 12.80226234 35.54158380147117 139.77616117114775 3.63726253 35.541562325766385 139.77617596202208 3.63690265</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2387_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2387_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2387_2">
											<gml:posList>35.54158379826629 139.7761611913429 12.80226234 35.54159182929704 139.77617700107498 12.80201066 35.541591829299655 139.77617700105887 12.79466987 35.54159183251357 139.77617698090253 3.63701085 35.54158380147117 139.77616117114775 3.63726253 35.54158379826629 139.7761611913429 12.80226234</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2387_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2387_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2387_3">
											<gml:posList>35.54156940548304 139.77619353774003 12.76470467 35.541591829299655 139.77617700105887 12.79466987 35.54159182929704 139.77617700107498 12.80201066 35.541569405470185 139.77619353782123 12.80161513 35.54156940548304 139.77619353774003 12.76470467</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2387_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2387_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2387_4">
											<gml:posList>35.541569405470185 139.77619353782123 12.80161513 35.541561029731895 139.77617687262224 12.80188084 35.54156103020435 139.77617686961784 11.43688087 35.54156940594443 139.77619353482035 11.43661516 35.54156940548304 139.77619353774003 12.76470467 35.541569405470185 139.77619353782123 12.80161513</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2382_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_0">
											<gml:posList>35.54117483978965 139.77645262370697 15.08543772 35.54123474124008 139.77641263695517 15.08636221 35.54123474290711 139.77641262471718 9.43136232 35.54119340699945 139.77644021819108 9.4307234 35.54119340867157 139.77644020566427 3.63072351 35.54117484305853 139.77645259898878 3.63043794 35.54117483978965 139.77645262370697 15.08543772</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2382_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_1">
											<gml:posList>35.54120586438744 139.77651938254502 15.08436587 35.54117483978965 139.77645262370697 15.08543772 35.54117484305853 139.77645259898878 3.63043794 35.541202723055754 139.7765125912581 3.62947439 35.541202720581325 139.7765126096059 12.16947422 35.541203385514535 139.7765140404247 12.17379143 35.541205865227504 139.7765193763294 12.18990371 35.54120586438744 139.77651938254502 15.08436587</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2382_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_2">
											<gml:posList>35.54126576657494 139.77647938976978 12.19607298 35.54126181656615 139.77647088998697 12.1704266 35.541261817385525 139.7764708840823 9.43042665 35.54123474290711 139.77641262471718 9.43136232 35.54123474124008 139.77641263695517 15.08636221 35.541265765709106 139.77647939599214 15.08529052 35.54126576657494 139.77647938976978 12.19607298</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2382_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_3">
											<gml:posList>35.541205865227504 139.7765193763294 12.18990371 35.54121210265773 139.77651521259696 12.19053474 35.54126576657494 139.77647938976978 12.19607298 35.541265765709106 139.77647939599214 15.08529052 35.54120586438744 139.77651938254502 15.08436587 35.541205865227504 139.7765193763294 12.18990371</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2382_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_6">
											<gml:posList>35.541207075125676 139.77651417374312 13.08446216 35.541179169453606 139.77645412622093 13.08542624 35.54117916888155 139.77645413053614 15.0854262 35.541207074544765 139.77651417803946 15.08446212 35.541207075125676 139.77651417374312 13.08446216</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2382_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_7">
											<gml:posList>35.54123353166877 139.7764178371454 13.08626525 35.54123353107959 139.77641784147195 15.08626521 35.54117916888155 139.77645413053614 15.0854262 35.541179169453606 139.77645412622093 13.08542624 35.54123353166877 139.7764178371454 13.08626525</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2382_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_8">
											<gml:posList>35.54123353166877 139.7764178371454 13.08626525 35.54126143722336 139.77647788484393 13.08530129 35.541261436625426 139.77647788915175 15.08530125 35.54123353107959 139.77641784147195 15.08626521 35.54123353166877 139.7764178371454 13.08626525</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2382_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2382_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2382_9">
											<gml:posList>35.541207074544765 139.77651417803946 15.08446212 35.541261436625426 139.77647788915175 15.08530125 35.54126143722336 139.77647788484393 13.08530129 35.541207075125676 139.77651417374312 13.08446216 35.541207074544765 139.77651417803946 15.08446212</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2380_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_5">
											<gml:posList>35.541761156738005 139.77658904303513 13.24542141 35.541921214988626 139.7769256334283 12.16534953 35.54159359577978 139.77623666457848 12.17594067 35.541761156738005 139.77658904303513 13.24542141</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2380_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_6">
											<gml:posList>35.541528363019246 139.7772029904941 12.15884792 35.54136836284369 139.77686598811104 13.23892866 35.54120112448638 139.77651373638992 12.16944862 35.541528363019246 139.7772029904941 12.15884792</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2380_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_7">
											<gml:posList>35.54159359577978 139.77623666457848 12.17594067 35.541921214988626 139.7769256334283 12.16534953 35.54192121842839 139.77692561563276 3.6253497 35.541594660266796 139.77623887810077 3.63590521 35.54159465765587 139.77623889438246 11.06590506 35.541593596169655 139.77623666214578 11.06594069 35.54159359577978 139.77623666457848 12.17594067</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2380_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_8">
											<gml:posList>35.541921214988626 139.7769256334283 12.16534953 35.541528363019246 139.7772029904941 12.15884792 35.541528365931285 139.77720297306954 3.61884808 35.54192121842839 139.77692561563276 3.6253497 35.541921214988626 139.7769256334283 12.16534953</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2380_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_9">
											<gml:posList>35.541528365931285 139.77720297306954 3.61884808 35.541528363019246 139.7772029904941 12.15884792 35.54120112448638 139.77651373638992 12.16944862 35.541201126958725 139.77651371804367 3.62944879 35.541528365931285 139.77720297306954 3.61884808</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2380_10">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_10">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_10">
											<gml:posList>35.541202720581325 139.7765126096059 12.16947422 35.541202723055754 139.7765125912581 3.62947439 35.541201126958725 139.77651371804367 3.62944879 35.54120112448638 139.77651373638992 12.16944862 35.541202720581325 139.7765126096059 12.16947422</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0878_p2380_11">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0878_p2380_11">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0878_p2380_11">
											<gml:posList>35.54159148438464 139.77623815372405 11.43590469 35.54158914238193 139.77623980711016 11.43586479 35.54158914308304 139.7762398027278 9.43586483 35.54158517073534 139.77624260708996 9.43579717 35.54155437218636 139.77626434996066 9.43527403 35.5415117075842 139.77629446988408 9.43455337 35.5414903172668 139.7763095707895 9.43419383 35.54136560091815 139.77639761650371 9.432121 35.541261817385525 139.7764708840823 9.43042665 35.54126181656615 139.77647088998697 12.1704266 35.54159359577978 139.77623666457848 12.17594067 35.541593596169655 139.77623666214578 11.06594069 35.541591484514505 139.77623815291327 11.0659047 35.54159148438464 139.77623815372405 11.43590469</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:address>
				<core:Address>
					<core:xalAddress>
						<xAL:AddressDetails>
							<xAL:Country>
								<xAL:CountryName>日本</xAL:CountryName>
								<xAL:Locality>
									<xAL:LocalityName Type="Town">東京都大田区羽田空港二丁目</xAL:LocalityName>
								</xAL:Locality>
							</xAL:Country>
						</xAL:AddressDetails>
					</core:xalAddress>
				</core:Address>
			</bldg:address>
			<uro:buildingDetails>
				<uro:BuildingDetails>
					<uro:buildingRoofEdgeArea uom="m2">3921.89152</uro:buildingRoofEdgeArea>
					<uro:districtsAndZonesType codeSpace="../../codelists/Common_districtsAndZonesType.xml">11</uro:districtsAndZonesType>
					<uro:prefecture codeSpace="../../codelists/Common_prefecture.xml">13</uro:prefecture>
					<uro:city codeSpace="../../codelists/Common_localPublicAuthorities.xml">13111</uro:city>
					<uro:surveyYear>2016</uro:surveyYear>
				</uro:BuildingDetails>
			</uro:buildingDetails>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">2</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key2.xml">2</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">106</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key106.xml">20</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
		</bldg:Building>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<bldg:Building gml:id="BLD_18b21c65-4d74-4792-81a5-36b268ee16d4">
			<gen:stringAttribute name="建物ID">
				<gen:value>13111-bldg-147299</gen:value>
			</gen:stringAttribute>
			<bldg:measuredHeight uom="m">5.1</bldg:measuredHeight>
			<bldg:lod0RoofEdge>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>35.54072854987352 139.77610967221491 8.246 35.541282342347145 139.7757201624373 8.246 35.54121227026244 139.77556832331607 8.246 35.540658473773824 139.77595784924029 8.246 35.54072854987352 139.77610967221491 8.246</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</bldg:lod0RoofEdge>
			<bldg:lod1Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54072854987352 139.77610967221491 3.109 35.540658473773824 139.77595784924029 3.109 35.54121227026244 139.77556832331607 3.109 35.541282342347145 139.7757201624373 3.109 35.54072854987352 139.77610967221491 3.109</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54072854987352 139.77610967221491 3.109 35.541282342347145 139.7757201624373 3.109 35.541282342347145 139.7757201624373 8.245999999999999 35.54072854987352 139.77610967221491 8.245999999999999 35.54072854987352 139.77610967221491 3.109</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.541282342347145 139.7757201624373 3.109 35.54121227026244 139.77556832331607 3.109 35.54121227026244 139.77556832331607 8.245999999999999 35.541282342347145 139.7757201624373 8.245999999999999 35.541282342347145 139.7757201624373 3.109</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54121227026244 139.77556832331607 3.109 35.540658473773824 139.77595784924029 3.109 35.540658473773824 139.77595784924029 8.245999999999999 35.54121227026244 139.77556832331607 8.245999999999999 35.54121227026244 139.77556832331607 3.109</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.540658473773824 139.77595784924029 3.109 35.54072854987352 139.77610967221491 3.109 35.54072854987352 139.77610967221491 8.245999999999999 35.540658473773824 139.77595784924029 8.245999999999999 35.540658473773824 139.77595784924029 3.109</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
							<gml:surfaceMember>
								<gml:Polygon>
									<gml:exterior>
										<gml:LinearRing>
											<gml:posList>35.54072854987352 139.77610967221491 8.245999999999999 35.541282342347145 139.7757201624373 8.245999999999999 35.54121227026244 139.77556832331607 8.245999999999999 35.540658473773824 139.77595784924029 8.245999999999999 35.54072854987352 139.77610967221491 8.245999999999999</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod1Solid>
			<bldg:lod2Solid>
				<gml:Solid>
					<gml:exterior>
						<gml:CompositeSurface>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_b_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_0"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_1"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_2"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_3"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_4"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_5"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_6"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_7"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_8"/>
							<gml:surfaceMember xlink:href="#poly_HNAP0276_p2397_9"/>
						</gml:CompositeSurface>
					</gml:exterior>
				</gml:Solid>
			</bldg:lod2Solid>
			<bldg:boundedBy>
				<bldg:GroundSurface gml:id="gnd_af26ccda-48af-4bf6-8215-9998356229b0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_b_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_b_0">
											<gml:posList>35.540728550363134 139.77610966323343 3.23922418 35.54065847420742 139.77595784471876 3.24181927 35.541212269965676 139.77556832345545 3.25076574 35.54128234342509 139.77572015764693 3.24816935 35.540728550363134 139.77610966323343 3.23922418</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:GroundSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0276_p2397_0">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_0">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_0">
											<gml:posList>35.540658473355975 139.77595785404228 7.41281918 35.54124779508743 139.77564530588657 7.93544452 35.54121226875094 139.7755683330335 7.42176565 35.540658473355975 139.77595785404228 7.41281918</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0276_p2397_1">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_1">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_1">
											<gml:posList>35.540695863413106 139.77603482458682 7.92650342 35.54124779508743 139.77564530588657 7.93544452 35.540658473355975 139.77595785404228 7.41281918 35.540695863413106 139.77603482458682 7.92650342</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0276_p2397_2">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_2">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_2">
											<gml:posList>35.54124779508743 139.77564530588657 7.93544452 35.540695863413106 139.77603482458682 7.92650342 35.54128234216433 139.77572016712585 7.41916926 35.54124779508743 139.77564530588657 7.93544452</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0276_p2397_3">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_3">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_3">
											<gml:posList>35.54128234216433 139.77572016712585 7.41916926 35.540695863413106 139.77603482458682 7.92650342 35.540728549465676 139.77610967245784 7.4102241 35.54128234216433 139.77572016712585 7.41916926</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:RoofSurface gml:id="roof_HNAP0276_p2397_4">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_4">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_4">
											<gml:posList>35.540695863413106 139.77603482458682 7.92650342 35.540658473355975 139.77595785404228 7.41281918 35.540728549465676 139.77610967245784 7.4102241 35.540695863413106 139.77603482458682 7.92650342</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:RoofSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0276_p2397_5">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_5">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_5">
											<gml:posList>35.54124779508743 139.77564530588657 7.93544452 35.54128234216433 139.77572016712585 7.41916926 35.54121226875094 139.7755683330335 7.42176565 35.54124779508743 139.77564530588657 7.93544452</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0276_p2397_6">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_6">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_6">
											<gml:posList>35.54128234216433 139.77572016712585 7.41916926 35.540728549465676 139.77610967245784 7.4102241 35.540728550363134 139.77610966323343 3.23922418 35.54128234342509 139.77572015764693 3.24816935 35.54128234216433 139.77572016712585 7.41916926</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0276_p2397_7">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_7">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_7">
											<gml:posList>35.540728549465676 139.77610967245784 7.4102241 35.540658473355975 139.77595785404228 7.41281918 35.54065847420742 139.77595784471876 3.24181927 35.540728550363134 139.77610966323343 3.23922418 35.540728549465676 139.77610967245784 7.4102241</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0276_p2397_8">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_8">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_8">
											<gml:posList>35.540658473355975 139.77595785404228 7.41281918 35.54121226875094 139.7755683330335 7.42176565 35.541212269965676 139.77556832345545 3.25076574 35.54065847420742 139.77595784471876 3.24181927 35.540658473355975 139.77595785404228 7.41281918</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:boundedBy>
				<bldg:WallSurface gml:id="wall_HNAP0276_p2397_9">
					<bldg:lod2MultiSurface>
						<gml:MultiSurface>
							<gml:surfaceMember>
								<gml:Polygon gml:id="poly_HNAP0276_p2397_9">
									<gml:exterior>
										<gml:LinearRing gml:id="line_HNAP0276_p2397_9">
											<gml:posList>35.54121226875094 139.7755683330335 7.42176565 35.54128234216433 139.77572016712585 7.41916926 35.54128234342509 139.77572015764693 3.24816935 35.541212269965676 139.77556832345545 3.25076574 35.54121226875094 139.7755683330335 7.42176565</gml:posList>
										</gml:LinearRing>
									</gml:exterior>
								</gml:Polygon>
							</gml:surfaceMember>
						</gml:MultiSurface>
					</bldg:lod2MultiSurface>
				</bldg:WallSurface>
			</bldg:boundedBy>
			<bldg:address>
				<core:Address>
					<core:xalAddress>
						<xAL:AddressDetails>
							<xAL:Country>
								<xAL:CountryName>日本</xAL:CountryName>
								<xAL:Locality>
									<xAL:LocalityName Type="Town">東京都大田区羽田空港二丁目</xAL:LocalityName>
								</xAL:Locality>
							</xAL:Country>
						</xAL:AddressDetails>
					</core:xalAddress>
				</core:Address>
			</bldg:address>
			<uro:buildingDetails>
				<uro:BuildingDetails>
					<uro:districtsAndZonesType codeSpace="../../codelists/Common_districtsAndZonesType.xml">11</uro:districtsAndZonesType>
					<uro:prefecture codeSpace="../../codelists/Common_prefecture.xml">13</uro:prefecture>
					<uro:city codeSpace="../../codelists/Common_localPublicAuthorities.xml">13111</uro:city>
				</uro:BuildingDetails>
			</uro:buildingDetails>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">2</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key2.xml">2</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
			<uro:extendedAttribute>
				<uro:KeyValuePair>
					<uro:key codeSpace="../../codelists/extendedAttribute_key.xml">106</uro:key>
					<uro:codeValue codeSpace="../../codelists/extendedAttribute_key106.xml">20</uro:codeValue>
				</uro:KeyValuePair>
			</uro:extendedAttribute>
		</bldg:Building>
	</core:cityObjectMember>
	<app:appearanceMember>
		<app:Appearance>
			<app:theme>rgbTexture</app:theme>
			<app:surfaceDataMember>
				<app:ParameterizedTexture>
					<app:imageURI>53392642_bldg_6697_appearance/hnap0876.tif</app:imageURI>
					<app:mimeType>image/tif</app:mimeType>
					<app:target uri="#poly_HNAP0876_p2401_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_3">0.7576057 0.6513783 0.7576057 0.6901396 0.7476923 0.6901396 0.7476923 0.6513783 0.7576057 0.6513783</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_3">0.7576057 0.6513783 0.7576057 0.6901396 0.7476923 0.6901396 0.7476923 0.6513783 0.7576057 0.6513783</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2444_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_4">0.7496633 0.5187707 0.7491706 0.5187707 0.7491706 0.46875 0.7496633 0.46875 0.7496633 0.5187707</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_4">0.7496633 0.5187707 0.7491706 0.5187707 0.7491706 0.46875 0.7496633 0.46875 0.7496633 0.5187707</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_2">0.5619208 0.109375 0.5619208 0.11057 0.1193928 0.11057 0.0716546 0.11057 0.0716546 0.109375 0.0726933 0.109375 0.1193928 0.109375 0.5221868 0.109375 0.5594164 0.109375 0.5619208 0.109375</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_2">0.5619208 0.109375 0.5619208 0.11057 0.1193928 0.11057 0.0716546 0.11057 0.0716546 0.109375 0.0726933 0.109375 0.1193928 0.109375 0.5221868 0.109375 0.5594164 0.109375 0.5619208 0.109375</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_4">0.729573 1 0.0011806 1 0.7273498 0.6828687 0.729573 1</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_4">0.729573 1 0.0011806 1 0.7273498 0.6828687 0.729573 1</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_7">0.8945202 0.1380208 0.8945202 0.990136 0.8651282 0.990136 0.8651282 0.1380208 0.8945202 0.1380208</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_7">0.8945202 0.1380208 0.8945202 0.990136 0.8651282 0.990136 0.8651282 0.1380208 0.8945202 0.1380208</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2443_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_4">0.749637 0.6222701 0.7491509 0.6222701 0.7491509 0.5729166 0.749637 0.5729166 0.749637 0.6222701</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_4">0.749637 0.6222701 0.7491509 0.6222701 0.7491509 0.5729166 0.749637 0.5729166 0.749637 0.6222701</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2444_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_1">0.7481851 0.5187707 0.7476923 0.5187707 0.7476923 0.46875 0.7481851 0.46875 0.7481851 0.5187707</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_1">0.7481851 0.5187707 0.7476923 0.5187707 0.7476923 0.46875 0.7481851 0.46875 0.7481851 0.5187707</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_3">0.0011806 1 0.0006582 0.6855043 0.7273498 0.6828687 0.0011806 1</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_3">0.0011806 1 0.0006582 0.6855043 0.7273498 0.6828687 0.0011806 1</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_1">0 0.2859908 0.7284152 0.2860227 0.0006582 0.6855043 0 0.2859908</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_1">0 0.2859908 0.7284152 0.2860227 0.0006582 0.6855043 0 0.2859908</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_0">7.4e-5 0.1144174 0.6495226 0.1141739 0.6495941 0.129433 0.5833369 0.1304258 0.5834544 0.1555281 0.6497118 0.1545354 0.6503056 0.28125 0 0.28125 7.4e-5 0.1144174</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_0">7.4e-5 0.1144174 0.6495226 0.1141739 0.6495941 0.129433 0.5833369 0.1304258 0.5834544 0.1555281 0.6497118 0.1545354 0.6503056 0.28125 0 0.28125 7.4e-5 0.1144174</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_3">0.7579548 0.9693134 0.7476923 0.9693134 0.7476923 0.9513889 0.7579548 0.9513889 0.7579548 0.9693134</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_3">0.7579548 0.9693134 0.7476923 0.9693134 0.7476923 0.9513889 0.7579548 0.9513889 0.7579548 0.9693134</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_5">0.7410256 0.6986252 0.7445546 0.3247504 0.7445546 0.9973958 0.7410256 0.6986252</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_5">0.7410256 0.6986252 0.7445546 0.3247504 0.7445546 0.9973958 0.7410256 0.6986252</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2444_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_2">0.7506488 0.5187707 0.7501561 0.5187707 0.7501561 0.46875 0.7506488 0.46875 0.7506488 0.5187707</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_2">0.7506488 0.5187707 0.7501561 0.5187707 0.7501561 0.46875 0.7506488 0.46875 0.7506488 0.5187707</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2443_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_0">0.7486647 0.6222701 0.7481785 0.6222701 0.7481785 0.5729166 0.7486647 0.5729166 0.7486647 0.6222701</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_0">0.7486647 0.6222701 0.7481785 0.6222701 0.7481785 0.5729166 0.7486647 0.5729166 0.7486647 0.6222701</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_8">0.8061854 0.9131944 0.851924 0.9131944 0.851924 0.9495426 0.8061854 0.9495426 0.8061854 0.9131944</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_8">0.8061854 0.9131944 0.851924 0.9131944 0.851924 0.9495426 0.8061854 0.9495426 0.8061854 0.9131944</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2443_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_5">0.7491509 0.6222701 0.7486647 0.6222701 0.7486647 0.5729166 0.7491509 0.5729166 0.7491509 0.6222701</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_5">0.7491509 0.6222701 0.7486647 0.6222701 0.7486647 0.5729166 0.7491509 0.5729166 0.7491509 0.6222701</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_1">0.8140482 0.9982639 0.7476923 0.9982639 0.747706 0.9731244 0.8140618 0.9731244 0.8140482 0.9982639</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_1">0.8140482 0.9982639 0.7476923 0.9982639 0.747706 0.9731244 0.8140618 0.9731244 0.8140482 0.9982639</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2442_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_4">0.7496302 0.5700127 0.7491457 0.5700127 0.7491457 0.5208334 0.7496302 0.5208334 0.7496302 0.5700127</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_4">0.7496302 0.5700127 0.7491457 0.5700127 0.7491457 0.5208334 0.7496302 0.5208334 0.7496302 0.5700127</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_8">0.7574989 0.4500622 0.7572408 0.450085 0.7576791 0.4635652 0.7520157 0.4586803 0.7522904 0.4583784 0.7522733 0.4578248 0.7519814 0.4575731 0.7517068 0.457875 0.7517239 0.4584286 0.7487179 0.4558359 0.7503583 0.4556907 0.7500967 0.4496528 0.7574855 0.4496528 0.7574989 0.4500622</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_8">0.7574989 0.4500622 0.7572408 0.450085 0.7576791 0.4635652 0.7520157 0.4586803 0.7522904 0.4583784 0.7522733 0.4578248 0.7519814 0.4575731 0.7517068 0.457875 0.7517239 0.4584286 0.7487179 0.4558359 0.7503583 0.4556907 0.7500967 0.4496528 0.7574855 0.4496528 0.7574989 0.4500622</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2401_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_0">0.8291448 0.9962463 0.8480703 0.9963928 0.8480654 0.9845774 0.8624365 0.9847463 0.8624431 0.9978497 0.8291458 0.9974571 0.8291448 0.9962463</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_0">0.8291448 0.9962463 0.8480703 0.9963928 0.8480654 0.9845774 0.8624365 0.9847463 0.8624431 0.9978497 0.8291458 0.9974571 0.8291448 0.9962463</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_6">0.7502097 0.9495426 0.7502097 0.9131944 0.7959483 0.9131944 0.7959483 0.9495426 0.7502097 0.9495426</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_6">0.7502097 0.9495426 0.7502097 0.9131944 0.7959483 0.9131944 0.7959483 0.9495426 0.7502097 0.9495426</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2442_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_2">0.7505991 0.5700127 0.7501147 0.5700127 0.7501147 0.5208334 0.7505991 0.5208334 0.7505991 0.5700127</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_2">0.7505991 0.5700127 0.7501147 0.5700127 0.7501147 0.5208334 0.7505991 0.5208334 0.7505991 0.5700127</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_9">0.0716546 0.109375 0.0716546 0.11057 0.0120795 0.11057 0.0120795 0.109375 0.01385 0.109375 0.0716546 0.109375</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_9">0.0716546 0.109375 0.0716546 0.11057 0.0120795 0.11057 0.0120795 0.109375 0.01385 0.109375 0.0716546 0.109375</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_9">0.7476923 0.9495426 0.7476923 0.9131944 0.7502097 0.9131944 0.7502097 0.9495426 0.7476923 0.9495426</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_9">0.7476923 0.9495426 0.7476923 0.9131944 0.7502097 0.9131944 0.7502097 0.9495426 0.7476923 0.9495426</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_0">0.9338461 0.5022948 0.9632381 0.5022948 0.9632381 0.9956597 0.9416557 0.9956597 0.9416557 0.9951789 0.9409359 0.9951789 0.9409359 0.9956597 0.9338461 0.9956597 0.9338461 0.5022948</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_0">0.9338461 0.5022948 0.9632381 0.5022948 0.9632381 0.9956597 0.9416557 0.9956597 0.9416557 0.9951789 0.9409359 0.9951789 0.9409359 0.9956597 0.9338461 0.9956597 0.9338461 0.5022948</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2401_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_7">0.7589743 0.7482639 0.759465 0.7482639 0.759465 0.7844428 0.7589743 0.7844428 0.7589743 0.7482639</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_7">0.7589743 0.7482639 0.759465 0.7482639 0.759465 0.7844428 0.7589743 0.7844428 0.7589743 0.7482639</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2401_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_2">0.7576057 0.6901396 0.7576057 0.6909722 0.7476923 0.6909722 0.7476923 0.6901396 0.7576057 0.6901396</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_2">0.7576057 0.6901396 0.7576057 0.6909722 0.7476923 0.6909722 0.7476923 0.6901396 0.7576057 0.6901396</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2439_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_0">0.7526606 0.6220679 0.7522277 0.6220679 0.7522277 0.578125 0.7526606 0.578125 0.7526606 0.6220679</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_0">0.7526606 0.6220679 0.7522277 0.6220679 0.7522277 0.578125 0.7526606 0.578125 0.7526606 0.6220679</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2442_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_1">0.7481768 0.5700127 0.7476923 0.5700127 0.7476923 0.5208334 0.7481768 0.5208334 0.7481768 0.5700127</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_1">0.7481768 0.5700127 0.7476923 0.5700127 0.7476923 0.5208334 0.7481768 0.5208334 0.7481768 0.5700127</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2400_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_3">0.7476923 0.7450469 0.7476923 0.733452 0.7576416 0.733452 0.7576416 0.7369066 0.7477102 0.7369066 0.7477102 0.7450469 0.7476923 0.7450469</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_3">0.7476923 0.7450469 0.7476923 0.733452 0.7576416 0.733452 0.7576416 0.7369066 0.7477102 0.7369066 0.7477102 0.7450469 0.7476923 0.7450469</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_4">0.7476923 0.8488619 0.7476923 0.8449286 0.7488454 0.846063 0.7491285 0.8463416 0.7516852 0.8488568 0.7476923 0.8488619</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_4">0.7476923 0.8488619 0.7476923 0.8449286 0.7488454 0.846063 0.7491285 0.8463416 0.7516852 0.8488568 0.7476923 0.8488619</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2400_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_4">0.7323077 0.3177083 0.7323077 0.2844625 0.7418249 0.2844625 0.7418249 0.3177083 0.7323077 0.3177083</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_4">0.7323077 0.3177083 0.7323077 0.2844625 0.7418249 0.2844625 0.7418249 0.3177083 0.7323077 0.3177083</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_2">0.7284152 0.2860227 0.7273498 0.6828687 0.0006582 0.6855043 0.7284152 0.2860227</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_2">0.7284152 0.2860227 0.7273498 0.6828687 0.0006582 0.6855043 0.7284152 0.2860227</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2400_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_2">0.7476923 0.6937953 0.7576416 0.6937953 0.7576416 0.733452 0.7476923 0.733452 0.7476923 0.6937953</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_2">0.7476923 0.6937953 0.7576416 0.6937953 0.7576416 0.733452 0.7476923 0.733452 0.7476923 0.6937953</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2401_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_1">0.7576057 0.6256377 0.7576057 0.6423671 0.7476923 0.6423671 0.7476923 0.6256377 0.7576057 0.6256377</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_1">0.7576057 0.6256377 0.7576057 0.6423671 0.7476923 0.6423671 0.7476923 0.6256377 0.7576057 0.6256377</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2443_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_3">0.7501232 0.6222701 0.749637 0.6222701 0.749637 0.5729166 0.7501232 0.5729166 0.7501232 0.6222701</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_3">0.7501232 0.6222701 0.749637 0.6222701 0.749637 0.5729166 0.7501232 0.5729166 0.7501232 0.6222701</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2444_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_3">0.7501561 0.5187707 0.7496633 0.5187707 0.7496633 0.46875 0.7501561 0.46875 0.7501561 0.5187707</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_3">0.7501561 0.5187707 0.7496633 0.5187707 0.7496633 0.46875 0.7501561 0.46875 0.7501561 0.5187707</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2439_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_5">0.7530935 0.6220679 0.7526606 0.6220679 0.7526606 0.578125 0.7530935 0.578125 0.7530935 0.6220679</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_5">0.7530935 0.6220679 0.7526606 0.6220679 0.7526606 0.578125 0.7530935 0.578125 0.7530935 0.6220679</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2401_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_5">0.7876215 0.7482639 0.7974814 0.7482639 0.7974814 0.7844428 0.7876215 0.7844428 0.7876215 0.7482639</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_5">0.7876215 0.7482639 0.7974814 0.7482639 0.7974814 0.7844428 0.7876215 0.7844428 0.7876215 0.7482639</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_3">0.0856112 0.0124122 0.043778 0.0117854 0.0438232 0.0021512 0.4532162 0.0023046 0.4523872 0.0047191 0.4522243 0.0042382 0.4518967 0.0042366 0.4517322 0.0047159 0.4518951 0.0051968 0.4522226 0.0051984 0.4170498 0.1076389 0.0433288 0.1076389 0.0437037 0.0276343 0.085537 0.0282612 0.0856112 0.0124122</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_3">0.0856112 0.0124122 0.043778 0.0117854 0.0438232 0.0021512 0.4532162 0.0023046 0.4523872 0.0047191 0.4522243 0.0042382 0.4518967 0.0042366 0.4517322 0.0047159 0.4518951 0.0051968 0.4522226 0.0051984 0.4170498 0.1076389 0.0433288 0.1076389 0.0437037 0.0276343 0.085537 0.0282612 0.0856112 0.0124122</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_8">0.9676923 0.5022728 0.9970843 0.5022728 0.9970843 0.9956597 0.9676923 0.9956597 0.9676923 0.5022728</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_8">0.9676923 0.5022728 0.9970843 0.5022728 0.9970843 0.9956597 0.9676923 0.9956597 0.9676923 0.5022728</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2400_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_0">0.7477102 0.7450469 0.7477102 0.7671178 0.7576416 0.7671178 0.7576416 0.7847222 0.7476923 0.7847222 0.7476923 0.7450469 0.7477102 0.7450469</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_0">0.7477102 0.7450469 0.7477102 0.7671178 0.7576416 0.7671178 0.7576416 0.7847222 0.7476923 0.7847222 0.7476923 0.7450469 0.7477102 0.7450469</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_0">0.811546 0.9524218 0.8046784 0.9524218 0.8046784 0.9693134 0.8038071 0.9693134 0.8038071 0.9513889 0.8140696 0.9513889 0.8140696 0.9693134 0.811546 0.9693134 0.811546 0.9524218</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_0">0.811546 0.9524218 0.8046784 0.9524218 0.8046784 0.9693134 0.8038071 0.9693134 0.8038071 0.9513889 0.8140696 0.9513889 0.8140696 0.9693134 0.811546 0.9693134 0.811546 0.9524218</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_4">0.8038071 0.9693134 0.7579548 0.9693134 0.7579548 0.9513889 0.8038071 0.9513889 0.8038071 0.9693134</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_4">0.8038071 0.9693134 0.7579548 0.9693134 0.7579548 0.9513889 0.8038071 0.9513889 0.8038071 0.9693134</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_5">0.8527932 0.9131944 0.8527932 0.9495426 0.851924 0.9495426 0.851924 0.9131944 0.8527932 0.9131944</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_5">0.8527932 0.9131944 0.8527932 0.9495426 0.851924 0.9495426 0.851924 0.9131944 0.8527932 0.9131944</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_7">0.8208845 0.9071183 0.81908 0.909539 0.8057045 0.8811779 0.8042389 0.8831375 0.7985685 0.8711752 0.7947383 0.8762948 0.8038162 0.8954448 0.7510628 0.909089 0.7511818 0.9085725 0.750977 0.9081398 0.7506533 0.9082235 0.7505342 0.90874 0.750739 0.9091727 0.7493225 0.909539 0.7487179 0.9082686 0.7905852 0.8515298 0.7939202 0.8506672 0.7942439 0.8505835 0.8005273 0.8489583 0.8253453 0.9011341 0.8208845 0.9071183</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_7">0.8208845 0.9071183 0.81908 0.909539 0.8057045 0.8811779 0.8042389 0.8831375 0.7985685 0.8711752 0.7947383 0.8762948 0.8038162 0.8954448 0.7510628 0.909089 0.7511818 0.9085725 0.750977 0.9081398 0.7506533 0.9082235 0.7505342 0.90874 0.750739 0.9091727 0.7493225 0.909539 0.7487179 0.9082686 0.7905852 0.8515298 0.7939202 0.8506672 0.7942439 0.8505835 0.8005273 0.8489583 0.8253453 0.9011341 0.8208845 0.9071183</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_9">0.9293919 0.1380208 0.9293919 0.9901093 0.9 0.9901093 0.9 0.1380208 0.9070898 0.1380208 0.9070898 0.2264648 0.9070898 0.9891758 0.9078095 0.9891758 0.9078095 0.9879613 0.9078095 0.2264648 0.9078095 0.1528561 0.9078095 0.1380208 0.9293919 0.1380208</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_9">0.9293919 0.1380208 0.9293919 0.9901093 0.9 0.9901093 0.9 0.1380208 0.9070898 0.1380208 0.9070898 0.2264648 0.9070898 0.9891758 0.9078095 0.9891758 0.9078095 0.9879613 0.9078095 0.2264648 0.9078095 0.1528561 0.9078095 0.1380208 0.9293919 0.1380208</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_12">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_12">0 0.11057 0 0.109375 0.0002611 0.109375 0.0002611 0.11057 0 0.11057</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_12">0 0.11057 0 0.109375 0.0002611 0.109375 0.0002611 0.11057 0 0.11057</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_7">0.7959483 0.9131944 0.8061854 0.9131944 0.8061854 0.9495426 0.7959483 0.9495426 0.7959483 0.9131944</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_7">0.7959483 0.9131944 0.8061854 0.9131944 0.8061854 0.9495426 0.7959483 0.9495426 0.7959483 0.9131944</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2444_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_0">0.7486778 0.5187707 0.7481851 0.5187707 0.7481851 0.46875 0.7486778 0.46875 0.7486778 0.5187707</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_0">0.7486778 0.5187707 0.7481851 0.5187707 0.7481851 0.46875 0.7486778 0.46875 0.7486778 0.5187707</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_1">0.7252151 0.1134527 0.7258609 0.1332318 0.720197 0.1330592 0.7204581 0.28125 0.6503056 0.28125 0.6497118 0.1545354 0.6497018 0.152404 0.6647754 0.1522948 0.6647828 0.1535038 0.6980326 0.1533977 0.6979575 0.1403139 0.6836069 0.1403602 0.6835787 0.1353533 0.649623 0.135606 0.6495941 0.129433 0.6495226 0.1141739 0.7248327 0.1141457 0.7248314 0.1134527 0.7252151 0.1134527</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_1">0.7252151 0.1134527 0.7258609 0.1332318 0.720197 0.1330592 0.7204581 0.28125 0.6503056 0.28125 0.6497118 0.1545354 0.6497018 0.152404 0.6647754 0.1522948 0.6647828 0.1535038 0.6980326 0.1533977 0.6979575 0.1403139 0.6836069 0.1403602 0.6835787 0.1353533 0.649623 0.135606 0.6495941 0.129433 0.6495226 0.1141739 0.7248327 0.1141457 0.7248314 0.1134527 0.7252151 0.1134527</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2442_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_0">0.7486612 0.5700127 0.7481768 0.5700127 0.7481768 0.5208334 0.7486612 0.5208334 0.7486612 0.5700127</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_0">0.7486612 0.5700127 0.7481768 0.5700127 0.7481768 0.5208334 0.7486612 0.5208334 0.7486612 0.5700127</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2377_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_6">0.7359608 0.7011163 0.7328205 0.9973958 0.7328205 0.3247204 0.7359608 0.7011163</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2377_6">0.7359608 0.7011163 0.7328205 0.9973958 0.7328205 0.3247204 0.7359608 0.7011163</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2442_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_5">0.7491457 0.5700127 0.7486612 0.5700127 0.7486612 0.5208334 0.7491457 0.5208334 0.7491457 0.5700127</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_5">0.7491457 0.5700127 0.7486612 0.5700127 0.7486612 0.5208334 0.7491457 0.5208334 0.7491457 0.5700127</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2442_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_3">0.7501147 0.5700127 0.7496302 0.5700127 0.7496302 0.5208334 0.7501147 0.5208334 0.7501147 0.5700127</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2442_3">0.7501147 0.5700127 0.7496302 0.5700127 0.7496302 0.5208334 0.7501147 0.5208334 0.7501147 0.5700127</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2439_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_4">0.7535264 0.6220679 0.7530935 0.6220679 0.7530935 0.578125 0.7535264 0.578125 0.7535264 0.6220679</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_4">0.7535264 0.6220679 0.7530935 0.6220679 0.7530935 0.578125 0.7535264 0.578125 0.7535264 0.6220679</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2444_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_5">0.7491706 0.5187707 0.7486778 0.5187707 0.7486778 0.46875 0.7491706 0.46875 0.7491706 0.5187707</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2444_5">0.7491706 0.5187707 0.7486778 0.5187707 0.7486778 0.46875 0.7491706 0.46875 0.7491706 0.5187707</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2439_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_1">0.7522277 0.6220679 0.7517948 0.6220679 0.7517948 0.578125 0.7522277 0.578125 0.7522277 0.6220679</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_1">0.7522277 0.6220679 0.7517948 0.6220679 0.7517948 0.578125 0.7522277 0.578125 0.7522277 0.6220679</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2401_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_8">0.759465 0.7482639 0.7665256 0.7482639 0.7823104 0.7482639 0.7823104 0.7844428 0.759465 0.7844428 0.759465 0.7482639</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_8">0.759465 0.7482639 0.7665256 0.7482639 0.7823104 0.7482639 0.7823104 0.7844428 0.759465 0.7844428 0.759465 0.7482639</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2400_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_5">0.825641 0.9112449 0.825641 0.875 0.8276771 0.875 0.8276771 0.9112449 0.825641 0.9112449</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_5">0.825641 0.9112449 0.825641 0.875 0.8276771 0.875 0.8276771 0.9112449 0.825641 0.9112449</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2400_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_1">0.8480703 0.9963928 0.8140494 0.9961295 0.8140585 0.9793065 0.8480633 0.9795631 0.8480703 0.9963928</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_1">0.8480703 0.9963928 0.8140494 0.9961295 0.8140585 0.9793065 0.8480633 0.9795631 0.8480703 0.9963928</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_10">0.0120795 0.109375 0.0120795 0.11057 0.0082247 0.11057 0.0082247 0.109375 0.0120795 0.109375</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_10">0.0120795 0.109375 0.0120795 0.11057 0.0082247 0.11057 0.0082247 0.109375 0.0120795 0.109375</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2443_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_1">0.7481785 0.6222701 0.7476923 0.6222701 0.7476923 0.5729166 0.7481785 0.5729166 0.7481785 0.6222701</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_1">0.7481785 0.6222701 0.7476923 0.6222701 0.7476923 0.5729166 0.7481785 0.5729166 0.7481785 0.6222701</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_5">0.7476923 0.8449286 0.7476923 0.7864583 0.808211 0.8459957 0.8084942 0.8462743 0.8099205 0.8476775 0.8099204 0.8487828 0.7516852 0.8488568 0.7491285 0.8463416 0.7494127 0.8460658 0.7494136 0.8455114 0.7491304 0.8452328 0.7488463 0.8455086 0.7488454 0.846063 0.7476923 0.8449286</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_5">0.7476923 0.8449286 0.7476923 0.7864583 0.808211 0.8459957 0.8084942 0.8462743 0.8099205 0.8476775 0.8099204 0.8487828 0.7516852 0.8488568 0.7491285 0.8463416 0.7494127 0.8460658 0.7494136 0.8455114 0.7491304 0.8452328 0.7488463 0.8455086 0.7488454 0.846063 0.7476923 0.8449286</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_13">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_13">0.6289898 0.109375 0.6289898 0.11057 0.5619208 0.11057 0.5619208 0.109375 0.5662243 0.109375 0.6289898 0.109375</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_13">0.6289898 0.109375 0.6289898 0.11057 0.5619208 0.11057 0.5619208 0.109375 0.5662243 0.109375 0.6289898 0.109375</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2399_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_2">0.8599218 0.9513889 0.8599218 0.9693134 0.8140696 0.9693134 0.8140696 0.9513889 0.8599218 0.9513889</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2399_2">0.8599218 0.9513889 0.8599218 0.9693134 0.8140696 0.9693134 0.8140696 0.9513889 0.8599218 0.9513889</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2401_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_6">0.7823104 0.7482639 0.7876215 0.7482639 0.7876215 0.7844428 0.7823104 0.7844428 0.7823104 0.7482639</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_6">0.7823104 0.7482639 0.7876215 0.7482639 0.7876215 0.7844428 0.7823104 0.7844428 0.7823104 0.7482639</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2401_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_4">0.7576057 0.6423671 0.7576057 0.6513783 0.7476923 0.6513783 0.7476923 0.6423671 0.7576057 0.6423671</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2401_4">0.7576057 0.6423671 0.7576057 0.6513783 0.7476923 0.6513783 0.7476923 0.6423671 0.7576057 0.6423671</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2439_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_3">0.7539592 0.6220679 0.7535264 0.6220679 0.7535264 0.578125 0.7539592 0.578125 0.7539592 0.6220679</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_3">0.7539592 0.6220679 0.7535264 0.6220679 0.7535264 0.578125 0.7539592 0.578125 0.7539592 0.6220679</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_11">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_11">0.0002611 0.109375 0.0082247 0.109375 0.0082247 0.11057 0.0002611 0.11057 0.0002611 0.109375</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_11">0.0002611 0.109375 0.0082247 0.109375 0.0082247 0.11057 0.0002611 0.11057 0.0002611 0.109375</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2379_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_6">0.0433288 0.1076389 0 0.1076389 0.00072 0.1055419 0.0008846 0.1050625 0.0277002 0.0269624 0.0341883 0.026983 0.0341929 0.0262198 0.04371 0.0262887 0.0437037 0.0276343 0.0433288 0.1076389</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2379_6">0.0433288 0.1076389 0 0.1076389 0.00072 0.1055419 0.0008846 0.1050625 0.0277002 0.0269624 0.0341883 0.026983 0.0341929 0.0262198 0.04371 0.0262887 0.0437037 0.0276343 0.0433288 0.1076389</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2443_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_2">0.7506094 0.6222701 0.7501232 0.6222701 0.7501232 0.5729166 0.7506094 0.5729166 0.7506094 0.6222701</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2443_2">0.7506094 0.6222701 0.7501232 0.6222701 0.7501232 0.5729166 0.7506094 0.5729166 0.7506094 0.6222701</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2439_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_2">0.7543922 0.6220679 0.7539592 0.6220679 0.7539592 0.578125 0.7543922 0.578125 0.7543922 0.6220679</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2439_2">0.7543922 0.6220679 0.7539592 0.6220679 0.7539592 0.578125 0.7543922 0.578125 0.7543922 0.6220679</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0876_p2400_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_6">0.8276771 0.9112449 0.8276771 0.875 0.85105 0.875 0.85105 0.9112449 0.8276771 0.9112449</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0876_p2400_6">0.8276771 0.9112449 0.8276771 0.875 0.85105 0.875 0.85105 0.9112449 0.8276771 0.9112449</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
				</app:ParameterizedTexture>
			</app:surfaceDataMember>
			<app:surfaceDataMember>
				<app:ParameterizedTexture>
					<app:imageURI>53392642_bldg_6697_appearance/hnap0878.tif</app:imageURI>
					<app:mimeType>image/tif</app:mimeType>
					<app:target uri="#poly_HNAP0878_p2382_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_9">0.9785156 0.8601612 0.9785156 0.7893808 0.9921169 0.7893808 0.9921169 0.8601612 0.9785156 0.8601612</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_9">0.9785156 0.8601612 0.9785156 0.7893808 0.9921169 0.7893808 0.9921169 0.8601612 0.9785156 0.8601612</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2396_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_0">0.4853443 0.181266 0.4853849 0.224534 0.4613816 0.2252787 0.4613412 0.1820139 0.4853443 0.181266</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_0">0.4853443 0.181266 0.4853849 0.224534 0.4613816 0.2252787 0.4613412 0.1820139 0.4853443 0.181266</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_0">0.4464375 0.1671783 0.446227 0.1361985 0.4853016 0.1358796 0.4853443 0.181266 0.4613412 0.1820139 0.4613816 0.2252787 0.4853849 0.224534 0.4853936 0.2337244 0.4468897 0.2337087 0.4466615 0.2001285 0.4464375 0.1671783</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_0">0.4464375 0.1671783 0.446227 0.1361985 0.4853016 0.1358796 0.4853443 0.181266 0.4613412 0.1820139 0.4613816 0.2252787 0.4853849 0.224534 0.4853936 0.2337244 0.4468897 0.2337087 0.4466615 0.2001285 0.4464375 0.1671783</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2392_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_1">0.9635985 0.210793 0.9045691 0.210793 0.9045691 0.1656805 0.9635985 0.1656805 0.9635985 0.210793</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_1">0.9635985 0.210793 0.9045691 0.210793 0.9045691 0.1656805 0.9635985 0.1656805 0.9635985 0.210793</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2395_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_3">0.3912725 0.1213018 0.3613281 0.1213018 0.3613281 0.099879 0.3912725 0.099879 0.3912725 0.1213018</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_3">0.3912725 0.1213018 0.3613281 0.1213018 0.3613281 0.099879 0.3912725 0.099879 0.3912725 0.1213018</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2391_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_3">0.2142617 0.1194597 0.2142617 0.0739645 0.2466034 0.0739645 0.2466034 0.1194597 0.2142617 0.1194597</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_3">0.2142617 0.1194597 0.2142617 0.0739645 0.2466034 0.0739645 0.2466034 0.1194597 0.2142617 0.1194597</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2388_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2388_3">0.7460938 0.2474952 0.7460938 0.2337278 0.7609667 0.2337278 0.7460938 0.2474952</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2388_3">0.7460938 0.2474952 0.7460938 0.2337278 0.7609667 0.2337278 0.7460938 0.2474952</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2389_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2389_0">0.4933127 0.1358142 0.4959918 0.1357924 0.4959877 0.1322787 0.521214 0.1335813 0.5212754 0.1672843 0.493465 0.1655503 0.4933127 0.1358142</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2389_0">0.4933127 0.1358142 0.4959918 0.1357924 0.4959877 0.1322787 0.521214 0.1335813 0.5212754 0.1672843 0.493465 0.1655503 0.4933127 0.1358142</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2392_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_0">0.3653931 0.2009793 0.2791243 0.2033066 0.2788348 0.16043 0.3651031 0.1580994 0.3653931 0.2009793</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_0">0.3653931 0.2009793 0.2791243 0.2033066 0.2788348 0.16043 0.3651031 0.1580994 0.3653931 0.2009793</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2394_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_3">0.9041165 0.2504358 0.8339465 0.2504358 0.8339465 0.2159763 0.9072667 0.2159763 0.9072667 0.2504358 0.9041165 0.2504358</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_3">0.9041165 0.2504358 0.8339465 0.2504358 0.8339465 0.2159763 0.9072667 0.2159763 0.9072667 0.2504358 0.9041165 0.2504358</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2391_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_0">0.4464375 0.1671783 0.4466615 0.2001285 0.3999046 0.2017354 0.3996808 0.1687852 0.4464375 0.1671783</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_0">0.4464375 0.1671783 0.4466615 0.2001285 0.3999046 0.2017354 0.3996808 0.1687852 0.4464375 0.1671783</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2392_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_4">0.9045691 0.1656805 0.9045691 0.210793 0.8852016 0.210793 0.8852016 0.1656805 0.9045691 0.1656805</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_4">0.9045691 0.1656805 0.9045691 0.210793 0.8852016 0.210793 0.8852016 0.1656805 0.9045691 0.1656805</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2395_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_2">0.3912725 0.0577767 0.3613281 0.0577767 0.3613281 0.037083 0.3912725 0.037083 0.3912725 0.0577767</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_2">0.3912725 0.0577767 0.3613281 0.0577767 0.3613281 0.037083 0.3912725 0.037083 0.3912725 0.0577767</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_10">0.07153 0.1313312 0.0035063 0.1343479 0 0.12925 0.074955 0.1259258 0.07153 0.1313312</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_10">0.07153 0.1313312 0.0035063 0.1343479 0 0.12925 0.074955 0.1259258 0.07153 0.1313312</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_6">0.9921169 0.8601612 0.9921169 0.9246899 0.9785156 0.9246899 0.9785156 0.8601612 0.9921169 0.8601612</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_6">0.9921169 0.8601612 0.9921169 0.9246899 0.9785156 0.9246899 0.9785156 0.8601612 0.9921169 0.8601612</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_3">0.0090502 0.356693 0.0072419 0.2545062 0.3694343 0.997721 0.001455 0.9973426 0.000225 0.3568166 0.0090502 0.356693</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_3">0.0090502 0.356693 0.0072419 0.2545062 0.3694343 0.997721 0.001455 0.9973426 0.000225 0.3568166 0.0090502 0.356693</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_6">0.9658203 0.2719327 0.9728957 0.6242977 0.9658203 0.9926035 0.9658203 0.2719327</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_6">0.9658203 0.2719327 0.9728957 0.6242977 0.9658203 0.9926035 0.9658203 0.2719327</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2394_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_1">0.9883505 0.2504358 0.9150358 0.2504358 0.9150358 0.2159763 0.9883505 0.2159763 0.9883505 0.2504358</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_1">0.9883505 0.2504358 0.9150358 0.2504358 0.9150358 0.2159763 0.9883505 0.2159763 0.9883505 0.2504358</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_8">0.9145933 0.4837278 0.9145933 0.9853329 0.8583984 0.9853329 0.8583984 0.4837278 0.9145933 0.4837278</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_8">0.9145933 0.4837278 0.9145933 0.9853329 0.8583984 0.9853329 0.8583984 0.4837278 0.9145933 0.4837278</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_1">0.051239 0.0038665 0.0983709 0.0038665 0.0983709 0.1213018 0.0560163 0.1213018 0.0560163 0.0337507 0.0550061 0.0337062 0.051239 0.0335402 0.051239 0.0038665</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_1">0.051239 0.0038665 0.0983709 0.0038665 0.0983709 0.1213018 0.0560163 0.1213018 0.0560163 0.0337507 0.0550061 0.0337062 0.051239 0.0335402 0.051239 0.0038665</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2387_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_2">0.3584496 0.1041228 0.3584496 0.1213018 0.3584003 0.1213018 0.296875 0.1213018 0.296875 0.1041228 0.3584496 0.1041228</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_2">0.3584496 0.1041228 0.3584496 0.1213018 0.3584003 0.1213018 0.296875 0.1213018 0.296875 0.1041228 0.3584496 0.1041228</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_7">0.7275391 0.9955621 0.7275391 0.2750786 0.783734 0.2750786 0.783734 0.9932278 0.7348431 0.9932278 0.7348431 0.9955621 0.7275391 0.9955621</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_7">0.7275391 0.9955621 0.7275391 0.2750786 0.783734 0.2750786 0.783734 0.9932278 0.7348431 0.9932278 0.7348431 0.9955621 0.7275391 0.9955621</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_5">0.0042335 0.2283225 0.0035063 0.1343479 0.07153 0.1313312 0.0722574 0.2253057 0.0042335 0.2283225</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_5">0.0042335 0.2283225 0.0035063 0.1343479 0.07153 0.1313312 0.0722574 0.2253057 0.0042335 0.2283225</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_5">0.9501953 0.6240206 0.9572707 0.2719467 0.9572707 0.9926035 0.9501953 0.6240206</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_5">0.9501953 0.6240206 0.9572707 0.2719467 0.9572707 0.9926035 0.9501953 0.6240206</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2391_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_1">0.2616464 0.1194597 0.2616464 0.0739645 0.2939881 0.0739645 0.2939881 0.1194597 0.2616464 0.1194597</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_1">0.2616464 0.1194597 0.2616464 0.0739645 0.2939881 0.0739645 0.2939881 0.1194597 0.2616464 0.1194597</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2396_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_2">0.997943 0.7218935 0.9785156 0.7218935 0.9785156 0.6920651 0.997943 0.6920651 0.997943 0.7218935</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_2">0.997943 0.7218935 0.9785156 0.7218935 0.9785156 0.6920651 0.997943 0.6920651 0.997943 0.7218935</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2389_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2389_1">0.9952122 0.4674309 0.9952122 0.5325444 0.9804688 0.5325444 0.9804688 0.4674309 0.9952122 0.4674309</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2389_1">0.9952122 0.4674309 0.9952122 0.5325444 0.9804688 0.5325444 0.9804688 0.4674309 0.9952122 0.4674309</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2396_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_1">0.997943 0.637161 0.9785156 0.637161 0.9785156 0.6120879 0.997943 0.6120879 0.997943 0.637161</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_1">0.997943 0.637161 0.9785156 0.637161 0.9785156 0.6120879 0.997943 0.6120879 0.997943 0.637161</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2393_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_4">0.9052261 0.1616547 0.9052261 0.1272189 0.9129887 0.1272189 0.9129887 0.1616547 0.9052261 0.1616547</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_4">0.9052261 0.1616547 0.9052261 0.1272189 0.9129887 0.1272189 0.9129887 0.1616547 0.9052261 0.1616547</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_9">0.9804688 0.6035503 0.9804688 0.5355781 0.9942235 0.5355781 0.9942235 0.6035503 0.9804688 0.6035503</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_9">0.9804688 0.6035503 0.9804688 0.5355781 0.9942235 0.5355781 0.9942235 0.6035503 0.9804688 0.6035503</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2387_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_3">0.3582016 0.056565 0.3584003 0.0270014 0.3584496 0.0270014 0.3584496 0.056565 0.3582016 0.056565</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_3">0.3582016 0.056565 0.3584003 0.0270014 0.3584496 0.0270014 0.3584496 0.056565 0.3582016 0.056565</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2396_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_3">0.997943 0.6920651 0.9785156 0.6920651 0.9785156 0.6669917 0.997943 0.6669917 0.997943 0.6920651</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_3">0.997943 0.6920651 0.9785156 0.6920651 0.9785156 0.6669917 0.997943 0.6669917 0.997943 0.6920651</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_0">0.0983709 0.0038665 0.14961 0.0038665 0.14961 0.0618409 0.1142517 0.0618409 0.1142517 0.1213018 0.0983709 0.1213018 0.0983709 0.0038665</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_0">0.0983709 0.0038665 0.14961 0.0038665 0.14961 0.0618409 0.1142517 0.0618409 0.1142517 0.1213018 0.0983709 0.1213018 0.0983709 0.0038665</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2395_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_0">0.3718546 0.1745101 0.3923497 0.173883 0.3925529 0.2038948 0.3720448 0.204523 0.3718546 0.1745101</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_0">0.3718546 0.1745101 0.3923497 0.173883 0.3925529 0.2038948 0.3720448 0.204523 0.3718546 0.1745101</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_2">0.1967419 0.0334864 0.1907411 0.0337507 0.1907411 0.0618409 0.14961 0.0618409 0.14961 0.0038665 0.1967419 0.0038665 0.1967419 0.0334864</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_2">0.1967419 0.0334864 0.1907411 0.0337507 0.1907411 0.0618409 0.14961 0.0618409 0.14961 0.0038665 0.1967419 0.0038665 0.1967419 0.0334864</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2393_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_2">0.8261719 0.1616547 0.8261719 0.1272189 0.8339466 0.1272189 0.8339466 0.1616547 0.8261719 0.1616547</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_2">0.8261719 0.1616547 0.8261719 0.1272189 0.8339466 0.1272189 0.8339466 0.1616547 0.8261719 0.1616547</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2393_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_0">0.2010974 0.1760483 0.200724 0.1941502 0.0909416 0.194452 0.0913136 0.176322 0.2010974 0.1760483</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_0">0.2010974 0.1760483 0.200724 0.1941502 0.0909416 0.194452 0.0913136 0.176322 0.2010974 0.1760483</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2394_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_2">0.8261719 0.2504358 0.8261719 0.2159763 0.8339465 0.2159763 0.8339465 0.2504358 0.8261719 0.2504358</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_2">0.8261719 0.2504358 0.8261719 0.2159763 0.8339465 0.2159763 0.8339465 0.2504358 0.8261719 0.2504358</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_7">0.9921169 0.9954705 0.9785156 0.9954705 0.9785156 0.9246899 0.9921169 0.9246899 0.9921169 0.9954705</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_7">0.9921169 0.9954705 0.9785156 0.9954705 0.9785156 0.9246899 0.9921169 0.9246899 0.9921169 0.9954705</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2393_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_1">0.9129887 0.1616547 0.9129887 0.1272189 0.984269 0.1272189 0.984269 0.1616547 0.9129887 0.1616547</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_1">0.9129887 0.1616547 0.9129887 0.1272189 0.984269 0.1272189 0.984269 0.1616547 0.9129887 0.1616547</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_7">0.8229664 0.2313334 0.5237904 0.2313334 0.5237904 0.1759147 0.6357925 0.1759147 0.7413217 0.1759147 0.7586062 0.1759147 0.7930824 0.1759147 0.8176036 0.1759147 0.821074 0.1759147 0.821074 0.1568047 0.8229664 0.1568047 0.8229664 0.2313334</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_7">0.8229664 0.2313334 0.5237904 0.2313334 0.5237904 0.1759147 0.6357925 0.1759147 0.7413217 0.1759147 0.7586062 0.1759147 0.7930824 0.1759147 0.8176036 0.1759147 0.821074 0.1759147 0.821074 0.1568047 0.8229664 0.1568047 0.8229664 0.2313334</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_8">0.9921169 0.7248521 0.9921169 0.7893808 0.9785156 0.7893808 0.9785156 0.7248521 0.9921169 0.7248521</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_8">0.9921169 0.7248521 0.9921169 0.7893808 0.9785156 0.7893808 0.9785156 0.7248521 0.9921169 0.7248521</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_11">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_11">0.7303016 0.2460647 0.7265625 0.2426035 0.7432176 0.2426035 0.7432176 0.2462559 0.7303016 0.2462559 0.7303016 0.2460647</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_11">0.7303016 0.2460647 0.7265625 0.2426035 0.7432176 0.2426035 0.7432176 0.2462559 0.7303016 0.2462559 0.7303016 0.2460647</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_6">0.5234375 0.2313334 0.5234375 0.1759147 0.5237904 0.1759147 0.5237904 0.2313334 0.5234375 0.2313334</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_6">0.5234375 0.2313334 0.5234375 0.1759147 0.5237904 0.1759147 0.5237904 0.2313334 0.5234375 0.2313334</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2391_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_2">0.1992188 0.1194597 0.1992188 0.0739645 0.2142617 0.0739645 0.2142617 0.1194597 0.1992188 0.1194597</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_2">0.1992188 0.1194597 0.1992188 0.0739645 0.2142617 0.0739645 0.2142617 0.1194597 0.1992188 0.1194597</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2388_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2388_1">0.2472722 0.0629741 0.1992188 0.0629741 0.1992188 0.0147929 0.2584458 0.0147929 0.247887 0.0409666 0.2472722 0.0409666 0.2472722 0.0629741</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2388_1">0.2472722 0.0629741 0.1992188 0.0629741 0.1992188 0.0147929 0.2584458 0.0147929 0.247887 0.0409666 0.2472722 0.0409666 0.2472722 0.0629741</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_9">0.7939453 0.2750646 0.8501402 0.2750646 0.8501402 0.9955621 0.7939453 0.9955621 0.7939453 0.2750646</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_9">0.7939453 0.2750646 0.8501402 0.2750646 0.8501402 0.9955621 0.7939453 0.9955621 0.7939453 0.2750646</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2391_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_4">0.2466034 0.1194597 0.2466034 0.0739645 0.2616464 0.0739645 0.2616464 0.1194597 0.2466034 0.1194597</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2391_4">0.2466034 0.1194597 0.2466034 0.0739645 0.2616464 0.0739645 0.2616464 0.1194597 0.2466034 0.1194597</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2392_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_3">0.8852016 0.1656805 0.8852016 0.210793 0.8261719 0.210793 0.8261719 0.1656805 0.8852016 0.1656805</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_3">0.8852016 0.1656805 0.8852016 0.210793 0.8261719 0.210793 0.8261719 0.1656805 0.8852016 0.1656805</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_5">0.075058 0.1392279 0.2067298 0.1381533 0.2065395 0.1474243 0.0935459 0.1491364 0.0931653 0.1672788 0.2013123 0.1656286 0.2010974 0.1760483 0.0913136 0.176322 0.0909416 0.194452 0.200724 0.1941502 0.1999101 0.2336084 0.0240424 0.233537 0.0240398 0.2326975 0.0757637 0.2304037 0.075058 0.1392279</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_5">0.075058 0.1392279 0.2067298 0.1381533 0.2065395 0.1474243 0.0935459 0.1491364 0.0931653 0.1672788 0.2013123 0.1656286 0.2010974 0.1760483 0.0913136 0.176322 0.0909416 0.194452 0.200724 0.1941502 0.1999101 0.2336084 0.0240424 0.233537 0.0240398 0.2326975 0.0757637 0.2304037 0.075058 0.1392279</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_4">0.3656143 0.2336757 0.1999101 0.2336084 0.200724 0.1941502 0.2010974 0.1760483 0.2013123 0.1656286 0.2061674 0.1655544 0.2065395 0.1474243 0.2067298 0.1381533 0.3649594 0.1368619 0.3651031 0.1580994 0.2788348 0.16043 0.2791243 0.2033066 0.3653931 0.2009793 0.3656143 0.2336757</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_4">0.3656143 0.2336757 0.1999101 0.2336084 0.200724 0.1941502 0.2010974 0.1760483 0.2013123 0.1656286 0.2061674 0.1655544 0.2065395 0.1474243 0.2067298 0.1381533 0.3649594 0.1368619 0.3651031 0.1580994 0.2788348 0.16043 0.2791243 0.2033066 0.3653931 0.2009793 0.3656143 0.2336757</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_3">0.051239 0.0335402 0.0459036 0.0335346 0 0.0334864 0 0.0038665 0.051239 0.0038665 0.051239 0.0335402</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_3">0.051239 0.0335402 0.0459036 0.0335346 0 0.0334864 0 0.0038665 0.051239 0.0038665 0.051239 0.0335402</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2394_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_4">0.9072667 0.2504358 0.9072667 0.2159763 0.9150358 0.2159763 0.9150358 0.2504358 0.9072667 0.2504358</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_4">0.9072667 0.2504358 0.9072667 0.2159763 0.9150358 0.2159763 0.9150358 0.2504358 0.9072667 0.2504358</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2395_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_1">0.3912725 0.079186 0.3613281 0.079186 0.3613281 0.0577767 0.3912725 0.0577767 0.3912725 0.079186</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_1">0.3912725 0.079186 0.3613281 0.079186 0.3613281 0.0577767 0.3912725 0.0577767 0.3912725 0.079186</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_1">0.3677029 0.2396449 0.3694343 0.997721 0.0072419 0.2545062 0.0070317 0.2426288 0.0014915 0.2427064 0 0.2396458 0.3677029 0.2396449</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_1">0.3677029 0.2396449 0.3694343 0.997721 0.0072419 0.2545062 0.0070317 0.2426288 0.0014915 0.2427064 0 0.2396458 0.3677029 0.2396449</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_10">0.7275391 0.25 0.783734 0.25 0.783734 0.2520379 0.7275391 0.2520379 0.7275391 0.25</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_10">0.7275391 0.25 0.783734 0.25 0.783734 0.2520379 0.7275391 0.2520379 0.7275391 0.25</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2394_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_0">0.2065395 0.1474243 0.2061674 0.1655544 0.0931653 0.1672788 0.0935459 0.1491364 0.2065395 0.1474243</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2394_0">0.2065395 0.1474243 0.2061674 0.1655544 0.0931653 0.1672788 0.0935459 0.1491364 0.2065395 0.1474243</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2387_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_1">0.5208911 0.2326841 0.4921821 0.2333811 0.492579 0.2067703 0.5213484 0.2073677 0.5208911 0.2326841</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_1">0.5208911 0.2326841 0.4921821 0.2333811 0.492579 0.2067703 0.5213484 0.2073677 0.5208911 0.2326841</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2395_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_4">0.3912725 0.079186 0.3912725 0.099879 0.3613281 0.099879 0.3613281 0.079186 0.3912725 0.079186</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2395_4">0.3912725 0.079186 0.3912725 0.099879 0.3613281 0.099879 0.3613281 0.079186 0.3912725 0.079186</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_2">0.3694343 0.997721 0.3677029 0.2396449 0.7209458 0.9980873 0.3694343 0.997721</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_2">0.3694343 0.997721 0.3677029 0.2396449 0.7209458 0.9980873 0.3694343 0.997721</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_0">5.9e-6 0.2427272 0 0.2396458 0.0014915 0.2427064 5.9e-6 0.2427272</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_0">5.9e-6 0.2427272 0 0.2396458 0.0014915 0.2427064 5.9e-6 0.2427272</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2392_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_2">0.9829645 0.1656805 0.9829645 0.210793 0.9635985 0.210793 0.9635985 0.1656805 0.9829645 0.1656805</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2392_2">0.9829645 0.1656805 0.9829645 0.210793 0.9635985 0.210793 0.9635985 0.1656805 0.9829645 0.1656805</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2388_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2388_2">0.2472722 0.068498 0.2472722 0.0409666 0.247887 0.0409666 0.2473975 0.068498 0.2472722 0.068498</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2388_2">0.2472722 0.068498 0.2472722 0.0409666 0.247887 0.0409666 0.2473975 0.068498 0.2472722 0.068498</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2387_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_4">0.3584496 0.056565 0.3584496 0.0746202 0.3492789 0.0746202 0.3492789 0.056565 0.3582016 0.056565 0.3584496 0.056565</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_4">0.3584496 0.056565 0.3584496 0.0746202 0.3492789 0.0746202 0.3492789 0.056565 0.3582016 0.056565 0.3584496 0.056565</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_2">0.3920977 0.1366403 0.446227 0.1361985 0.4464375 0.1671783 0.3996808 0.1687852 0.3999046 0.2017354 0.4466615 0.2001285 0.4468897 0.2337087 0.3927546 0.2336868 0.3925529 0.2038948 0.3923497 0.173883 0.3920977 0.1366403</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_2">0.3920977 0.1366403 0.446227 0.1361985 0.4464375 0.1671783 0.3996808 0.1687852 0.3999046 0.2017354 0.4466615 0.2001285 0.4468897 0.2337087 0.3927546 0.2336868 0.3925529 0.2038948 0.3923497 0.173883 0.3920977 0.1366403</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2393_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_3">0.8339466 0.1616547 0.8339466 0.1272189 0.9052261 0.1272189 0.9052261 0.1616547 0.8339466 0.1616547</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2393_3">0.8339466 0.1616547 0.8339466 0.1272189 0.9052261 0.1272189 0.9052261 0.1616547 0.8339466 0.1616547</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_11">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_11">0.9277209 0.5633471 0.9277209 0.5663373 0.9408813 0.5663373 0.9408813 0.5714093 0.9408813 0.6107333 0.9408813 0.665208 0.9408813 0.6925194 0.9408813 0.8517587 0.9408813 0.9842707 0.9228516 0.9842707 0.9228516 0.5606509 0.9301556 0.5606509 0.9301556 0.5633471 0.9277209 0.5633471</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_11">0.9277209 0.5633471 0.9277209 0.5663373 0.9408813 0.5663373 0.9408813 0.5714093 0.9408813 0.6107333 0.9408813 0.665208 0.9408813 0.6925194 0.9408813 0.8517587 0.9408813 0.9842707 0.9228516 0.9842707 0.9228516 0.5606509 0.9301556 0.5606509 0.9301556 0.5633471 0.9277209 0.5633471</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_10">0.4938143 0.2337278 0.4908428 0.2337266 0.4903414 0.1358385 0.4933127 0.1358142 0.4935091 0.1741586 0.4925679 0.1741629 0.492579 0.2067703 0.4921821 0.2333811 0.4938123 0.2333416 0.4938143 0.2337278</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_10">0.4938143 0.2337278 0.4908428 0.2337266 0.4903414 0.1358385 0.4933127 0.1358142 0.4935091 0.1741586 0.4925679 0.1741629 0.492579 0.2067703 0.4921821 0.2333811 0.4938123 0.2333416 0.4938143 0.2337278</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2388_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2388_0">0.291659 0.0222549 0.2917169 0.0680473 0.2636719 0.0673062 0.2636719 0.0300546 0.2645894 0.0300518 0.2645493 0.0202172 0.291659 0.0222549</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2388_0">0.291659 0.0222549 0.2917169 0.0680473 0.2636719 0.0673062 0.2636719 0.0300546 0.2645894 0.0300518 0.2645493 0.0202172 0.291659 0.0222549</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_1">0.4853443 0.181266 0.4853016 0.1358796 0.4903414 0.1358385 0.4908428 0.2337266 0.4853936 0.2337244 0.4853849 0.224534 0.4853443 0.181266</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_1">0.4853443 0.181266 0.4853016 0.1358796 0.4903414 0.1358385 0.4908428 0.2337266 0.4853936 0.2337244 0.4853849 0.224534 0.4853443 0.181266</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2382_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_4">0.0042335 0.2283225 0.0722574 0.2253057 0.07153 0.1313312 0.074955 0.1259258 0.0757637 0.2304037 0.0008085 0.2337278 0 0.12925 0.0035063 0.1343479 0.0042335 0.2283225</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2382_4">0.0042335 0.2283225 0.0722574 0.2253057 0.07153 0.1313312 0.074955 0.1259258 0.0757637 0.2304037 0.0008085 0.2337278 0 0.12925 0.0035063 0.1343479 0.0042335 0.2283225</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2396_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_4">0.997943 0.6669917 0.9785156 0.6669917 0.9785156 0.637161 0.997943 0.637161 0.997943 0.6669917</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2396_4">0.997943 0.6669917 0.9785156 0.6669917 0.9785156 0.637161 0.997943 0.637161 0.997943 0.6669917</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2380_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_4">0.7209458 0.9980873 0.3677029 0.2396449 0.7195049 0.2396449 0.7209458 0.9980873</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2380_4">0.7209458 0.9980873 0.3677029 0.2396449 0.7195049 0.2396449 0.7209458 0.9980873</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_3">0.3649594 0.1368619 0.3920977 0.1366403 0.3923497 0.173883 0.3718546 0.1745101 0.3720448 0.204523 0.3925529 0.2038948 0.3927546 0.2336868 0.3656143 0.2336757 0.3653931 0.2009793 0.3651031 0.1580994 0.3649594 0.1368619</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_3">0.3649594 0.1368619 0.3920977 0.1366403 0.3923497 0.173883 0.3718546 0.1745101 0.3720448 0.204523 0.3925529 0.2038948 0.3927546 0.2336868 0.3656143 0.2336757 0.3653931 0.2009793 0.3651031 0.1580994 0.3649594 0.1368619</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2387_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_0">0.296875 0.0762955 0.3492789 0.0762955 0.3492789 0.0746202 0.3584496 0.0746202 0.3584496 0.1041228 0.296875 0.1041228 0.296875 0.0762955</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2387_0">0.296875 0.0762955 0.3492789 0.0762955 0.3492789 0.0746202 0.3584496 0.0746202 0.3584496 0.1041228 0.296875 0.1041228 0.296875 0.0762955</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0878_p2390_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_8">0.8229664 0.2313334 0.8229664 0.1568047 0.8231288 0.1568047 0.8231288 0.2313334 0.8229664 0.2313334</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0878_p2390_8">0.8229664 0.2313334 0.8229664 0.1568047 0.8231288 0.1568047 0.8231288 0.2313334 0.8229664 0.2313334</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
				</app:ParameterizedTexture>
			</app:surfaceDataMember>
			<app:surfaceDataMember>
				<app:ParameterizedTexture>
					<app:imageURI>53392642_bldg_6697_appearance/hnap0285.tif</app:imageURI>
					<app:mimeType>image/tif</app:mimeType>
					<app:target uri="#poly_HNAP0285_p2524_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_0">0.4683193 0.8235294 0.0033344 0.8234677 0.0030675 0.0008819 0.4681583 0.0008819 0.4683193 0.8235294</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_0">0.4683193 0.8235294 0.0033344 0.8234677 0.0030675 0.0008819 0.4681583 0.0008819 0.4683193 0.8235294</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0285_p2524_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_3">0.1572536 0.8588235 0.4981768 0.8588235 0.4981768 0.9938665 0.1572536 0.9938665 0.1572536 0.8588235</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_3">0.1572536 0.8588235 0.4981768 0.8588235 0.4981768 0.9938665 0.1572536 0.9938665 0.1572536 0.8588235</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0285_p2524_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_1">0.6554421 0.8588235 0.9964428 0.8588235 0.9964428 0.9938665 0.6554421 0.9938665 0.6554421 0.8588235</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_1">0.6554421 0.8588235 0.9964428 0.8588235 0.9964428 0.9938665 0.6554421 0.9938665 0.6554421 0.8588235</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0285_p2524_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_2">0 0.8588235 0.1572536 0.8588235 0.1572536 0.9938665 0 0.9938665 0 0.8588235</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_2">0 0.8588235 0.1572536 0.8588235 0.1572536 0.9938665 0 0.9938665 0 0.8588235</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0285_p2524_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_4">0.4981768 0.8588235 0.6554421 0.8588235 0.6554421 0.9938665 0.4981768 0.9938665 0.4981768 0.8588235</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0285_p2524_4">0.4981768 0.8588235 0.6554421 0.8588235 0.6554421 0.9938665 0.4981768 0.9938665 0.4981768 0.8588235</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
				</app:ParameterizedTexture>
			</app:surfaceDataMember>
			<app:surfaceDataMember>
				<app:ParameterizedTexture>
					<app:imageURI>53392642_bldg_6697_appearance/hnap0276.tif</app:imageURI>
					<app:mimeType>image/tif</app:mimeType>
					<app:target uri="#poly_HNAP0276_p2397_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_0">0.0016283 0.9999639 0.955766 0.7041197 0.9565942 1 0.0016283 0.9999639</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_0">0.0016283 0.9999639 0.955766 0.7041197 0.9565942 1 0.0016283 0.9999639</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_4">0.9712953 0.7117097 0.9644737 0.9892086 0.9644737 0.4487081 0.9712953 0.7117097</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_4">0.9712953 0.7117097 0.9644737 0.9892086 0.9644737 0.4487081 0.9712953 0.7117097</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_8">0.6240727 0.384892 0.0039474 0.384892 0.0039474 0.2851213 0.6240727 0.2851213 0.6240727 0.384892</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_8">0.6240727 0.384892 0.0039474 0.384892 0.0039474 0.2851213 0.6240727 0.2851213 0.6240727 0.384892</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_3">0.9549527 0.4163261 0.003217 0.7003003 0 0.4163261 0.9549527 0.4163261</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_3">0.9549527 0.4163261 0.003217 0.7003003 0 0.4163261 0.9549527 0.4163261</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_1">0.003217 0.7003003 0.955766 0.7041197 0.0016283 0.9999639 0.003217 0.7003003</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_1">0.003217 0.7003003 0.955766 0.7041197 0.0016283 0.9999639 0.003217 0.7003003</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_7">0.1422908 0.0961736 0.0039474 0.0961736 0.0039474 0 0.1422908 0 0.1422908 0.0961736</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_7">0.1422908 0.0961736 0.0039474 0.0961736 0.0039474 0 0.1422908 0 0.1422908 0.0961736</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_5">0.9855264 0.7151809 0.9919661 0.4486737 0.9919661 0.9892086 0.9855264 0.7151809</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_5">0.9855264 0.7151809 0.9919661 0.4486737 0.9919661 0.9892086 0.9855264 0.7151809</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_6">0.0039474 0.1484306 0.6240641 0.1484306 0.6240641 0.2482014 0.0039474 0.2482014 0.0039474 0.1484306</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_6">0.0039474 0.1484306 0.6240641 0.1484306 0.6240641 0.2482014 0.0039474 0.2482014 0.0039474 0.1484306</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_2">0.955766 0.7041197 0.003217 0.7003003 0.9549527 0.4163261 0.955766 0.7041197</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_2">0.955766 0.7041197 0.003217 0.7003003 0.9549527 0.4163261 0.955766 0.7041197</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0276_p2397_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_9">0.1539474 0 0.2922997 0 0.2922997 0.0961736 0.1539474 0.0961736 0.1539474 0</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0276_p2397_9">0.1539474 0 0.2922997 0 0.2922997 0.0961736 0.1539474 0.0961736 0.1539474 0</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
				</app:ParameterizedTexture>
			</app:surfaceDataMember>
			<app:surfaceDataMember>
				<app:ParameterizedTexture>
					<app:imageURI>53392642_bldg_6697_appearance/hnap0275.tif</app:imageURI>
					<app:mimeType>image/tif</app:mimeType>
					<app:target uri="#poly_HNAP0275_p2417_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_4">0.6292135 0.473236 0.6292135 0.462033 0.631085 0.462033 0.631085 0.473236 0.6292135 0.473236</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_4">0.6292135 0.473236 0.6292135 0.462033 0.631085 0.462033 0.631085 0.473236 0.6292135 0.473236</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2419_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_2">0.6380119 0.5 0.6380119 0.4888206 0.6398778 0.4888206 0.6398778 0.5 0.6380119 0.5</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_2">0.6380119 0.5 0.6380119 0.4888206 0.6398778 0.4888206 0.6398778 0.5 0.6380119 0.5</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_33">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_33">0.3317179 0.9925402 0.2853761 0.9925402 0.2853761 0.9821227 0.3317179 0.9821227 0.3317179 0.9925402</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_33">0.3317179 0.9925402 0.2853761 0.9925402 0.2853761 0.9821227 0.3317179 0.9821227 0.3317179 0.9925402</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2414_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_4">0.6096727 0.5790755 0.6155444 0.5790755 0.6155444 0.5968539 0.6096727 0.5968539 0.6096727 0.5790755</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_4">0.6096727 0.5790755 0.6155444 0.5790755 0.6155444 0.5968539 0.6096727 0.5968539 0.6096727 0.5790755</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2462_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_3">0.7969609 0.6495351 0.795987 0.6494074 0.7941079 0.6496311 0.7941079 0.6413734 0.7969609 0.6413734 0.7969609 0.6495351</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_3">0.7969609 0.6495351 0.795987 0.6494074 0.7941079 0.6496311 0.7941079 0.6413734 0.7969609 0.6413734 0.7969609 0.6495351</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2425_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_1">0.8246214 0.6880867 0.8293648 0.6880867 0.8293648 0.698398 0.8246214 0.698398 0.8246214 0.6880867</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_1">0.8246214 0.6880867 0.8293648 0.6880867 0.8293648 0.698398 0.8246214 0.698398 0.8246214 0.6880867</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2454_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_4">0.649279 0.4882385 0.6525582 0.4882385 0.6525582 0.4975845 0.6492806 0.4975845 0.649279 0.4882385</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_4">0.649279 0.4882385 0.6525582 0.4882385 0.6525582 0.4975845 0.6492806 0.4975845 0.649279 0.4882385</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_5">0.5826839 0.5035669 0.5826723 0.5203937 0.5831376 0.5203983 0.5831376 0.5608273 0.582375 0.5608048 0.582375 0.4887669 0.5831376 0.4887826 0.5831376 0.5035713 0.5826839 0.5035669</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_5">0.5826839 0.5035669 0.5826723 0.5203937 0.5831376 0.5203983 0.5831376 0.5608273 0.582375 0.5608048 0.582375 0.4887669 0.5831376 0.4887826 0.5831376 0.5035713 0.5826839 0.5035669</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2467_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_0">0.6747664 0.5778942 0.6747664 0.5927005 0.6702545 0.5926653 0.6702492 0.5778589 0.6747664 0.5778942</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_0">0.6747664 0.5778942 0.6747664 0.5927005 0.6702545 0.5926653 0.6702492 0.5778589 0.6747664 0.5778942</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_0">0.8267779 0.8183817 0.8268543 0.8094902 0.825495 0.8094772 0.8258356 0.7469586 0.8295891 0.7469593 0.8296961 0.8217061 0.8297735 0.8757836 0.8297797 0.8801764 0.8251099 0.8801764 0.8254466 0.8183667 0.8267779 0.8183817</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_0">0.8267779 0.8183817 0.8268543 0.8094902 0.825495 0.8094772 0.8258356 0.7469586 0.8295891 0.7469593 0.8296961 0.8217061 0.8297735 0.8757836 0.8297797 0.8801764 0.8251099 0.8801764 0.8254466 0.8183667 0.8267779 0.8183817</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_7">0.48456 0.1905372 0.0031689 0.1905372 0.0031689 0.1825101 0.0014497 0.1825101 0.0007328 0.1825101 0.0007328 0.1465219 0.4877655 0.1465219 0.4877655 0.2110705 0.0007328 0.2110705 0.0007328 0.1905372 0.0014497 0.1905372 0.0014497 0.19134 0.4862792 0.19134 0.4862792 0.1825101 0.48456 0.1825101 0.48456 0.1905372</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_7">0.48456 0.1905372 0.0031689 0.1905372 0.0031689 0.1825101 0.0014497 0.1825101 0.0007328 0.1825101 0.0007328 0.1465219 0.4877655 0.1465219 0.4877655 0.2110705 0.0007328 0.2110705 0.0007328 0.1905372 0.0014497 0.1905372 0.0014497 0.19134 0.4862792 0.19134 0.4862792 0.1825101 0.48456 0.1825101 0.48456 0.1905372</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2456_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_4">0.6546417 0.5333604 0.6589954 0.5333604 0.6589954 0.5456446 0.6546439 0.5456446 0.6546417 0.5333604</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_4">0.6546417 0.5333604 0.6589954 0.5333604 0.6589954 0.5456446 0.6546439 0.5456446 0.6546417 0.5333604</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2414_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_2">0.6096727 0.6077393 0.6155444 0.6077393 0.6155444 0.6255174 0.6096727 0.6255174 0.6096727 0.6077393</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_2">0.6096727 0.6077393 0.6155444 0.6077393 0.6155444 0.6255174 0.6096727 0.6255174 0.6096727 0.6077393</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2422_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_0">0.2967113 0.3340397 0.5577155 0.3334163 0.5577156 0.3454988 0.0004885 0.3454988 0.0004885 0.334744 0.2432481 0.3341551 0.2432254 0.3303875 0.2683106 0.3305609 0.2683201 0.3326435 0.2718862 0.3326993 0.2718772 0.3305855 0.2966914 0.3307571 0.2967113 0.3340397</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_0">0.2967113 0.3340397 0.5577155 0.3334163 0.5577156 0.3454988 0.0004885 0.3454988 0.0004885 0.334744 0.2432481 0.3341551 0.2432254 0.3303875 0.2683106 0.3305609 0.2683201 0.3326435 0.2718862 0.3326993 0.2718772 0.3305855 0.2966914 0.3307571 0.2967113 0.3340397</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2457_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_3">0.6546533 0.5785127 0.6547293 0.5749185 0.6546186 0.5691509 0.6590113 0.5691509 0.6590113 0.5785127 0.6546533 0.5785127</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_3">0.6546533 0.5785127 0.6547293 0.5749185 0.6546186 0.5691509 0.6590113 0.5691509 0.6590113 0.5785127 0.6546533 0.5785127</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2465_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_2">0.653151 0.4718435 0.6564715 0.4718435 0.6564715 0.4812264 0.6531525 0.4812264 0.653151 0.4718435</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_2">0.653151 0.4718435 0.6564715 0.4718435 0.6564715 0.4812264 0.6531525 0.4812264 0.653151 0.4718435</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2456_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_3">0.6546417 0.5333604 0.6547233 0.5294982 0.6546184 0.524032 0.6589954 0.524032 0.6589954 0.5333604 0.6546417 0.5333604</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_3">0.6546417 0.5333604 0.6547233 0.5294982 0.6546184 0.524032 0.6589954 0.524032 0.6589954 0.5333604 0.6546417 0.5333604</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2460_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_4">0.6302712 0.5723542 0.6348706 0.5723542 0.6348706 0.5856307 0.6302736 0.5856307 0.6302712 0.5723542</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_4">0.6302712 0.5723542 0.6348706 0.5723542 0.6348706 0.5856307 0.6302736 0.5856307 0.6302712 0.5723542</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_1">0.8244063 0.8795618 0.8187591 0.8797686 0.8187591 0.6991678 0.8244352 0.6982968 0.8244153 0.8236325 0.8190554 0.8244584 0.8190534 0.8475767 0.8244116 0.8467525 0.8244063 0.8795618</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_1">0.8244063 0.8795618 0.8187591 0.8797686 0.8187591 0.6991678 0.8244352 0.6982968 0.8244153 0.8236325 0.8190554 0.8244584 0.8190534 0.8475767 0.8244116 0.8467525 0.8244063 0.8795618</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2417_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_3">0.631085 0.473236 0.631085 0.462033 0.6380465 0.462033 0.6380465 0.473236 0.631085 0.473236</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_3">0.631085 0.473236 0.631085 0.462033 0.6380465 0.462033 0.6380465 0.473236 0.631085 0.473236</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2427_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_3">0.6749511 0.6238121 0.6749511 0.6259534 0.6749511 0.6371882 0.6697606 0.6371882 0.6697606 0.6238121 0.6749511 0.6238121</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_3">0.6749511 0.6238121 0.6749511 0.6259534 0.6749511 0.6371882 0.6697606 0.6371882 0.6697606 0.6238121 0.6749511 0.6238121</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2463_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_1">0.7874939 0.65922 0.7874939 0.651085 0.7903701 0.651085 0.7903701 0.6593674 0.7882733 0.6591178 0.7874939 0.65922</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_1">0.7874939 0.65922 0.7874939 0.651085 0.7903701 0.651085 0.7903701 0.6593674 0.7882733 0.6591178 0.7874939 0.65922</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2422_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_1">0.494382 0.2026932 0.494382 0.2019465 0.4978883 0.2019465 0.4978883 0.2026932 0.494382 0.2026932</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_1">0.494382 0.2026932 0.494382 0.2019465 0.4978883 0.2019465 0.4978883 0.2026932 0.494382 0.2026932</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_16">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_16">0.5744992 0.6376358 0.6904321 0.6376358 0.6938398 0.6376358 0.712028 0.6376358 0.7121882 0.6453737 0.5750871 0.6581509 0.5744992 0.6376358</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_16">0.5744992 0.6376358 0.6904321 0.6376358 0.6938398 0.6376358 0.712028 0.6376358 0.7121882 0.6453737 0.5750871 0.6581509 0.5744992 0.6376358</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2412_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_2">0.641915 0.5629194 0.6460214 0.5629194 0.6460214 0.5770469 0.641915 0.5770469 0.641915 0.5629194</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_2">0.641915 0.5629194 0.6460214 0.5629194 0.6460214 0.5770469 0.641915 0.5770469 0.641915 0.5629194</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_3">0.6096727 0.9990565 0.6096727 0.9939173 0.621732 0.9939173 0.621732 0.9990565 0.6096727 0.9990565</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_3">0.6096727 0.9990565 0.6096727 0.9939173 0.621732 0.9939173 0.621732 0.9990565 0.6096727 0.9990565</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_10">0.4929645 0.2067732 0.4897411 0.2067732 0.4897411 0.2049878 0.4929645 0.2049878 0.4929645 0.2067732</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_10">0.4929645 0.2067732 0.4897411 0.2067732 0.4897411 0.2049878 0.4929645 0.2049878 0.4929645 0.2067732</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_22">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_22">0.5602909 0.9306569 0.5602909 0.9369777 0.3390423 0.9369777 0.3390423 0.9306569 0.5602909 0.9306569</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_22">0.5602909 0.9306569 0.5602909 0.9369777 0.3390423 0.9369777 0.3390423 0.9306569 0.5602909 0.9306569</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_3">0.8695652 0.9555994 0.8695652 0.9501216 0.8713909 0.9501216 0.8713909 0.9555994 0.8695652 0.9555994</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_3">0.8695652 0.9555994 0.8695652 0.9501216 0.8713909 0.9501216 0.8713909 0.9555994 0.8695652 0.9555994</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2455_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_3">0.6492794 0.5312248 0.649356 0.5276009 0.6492448 0.5218072 0.6536481 0.5218072 0.6536481 0.5312248 0.6492794 0.5312248</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_3">0.6492794 0.5312248 0.649356 0.5276009 0.6492448 0.5218072 0.6536481 0.5218072 0.6536481 0.5312248 0.6492794 0.5312248</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2462_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_1">0.7874939 0.6495311 0.7874939 0.6413734 0.7903485 0.6413734 0.7903485 0.6496351 0.7884368 0.6494074 0.7874939 0.6495311</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_1">0.7874939 0.6495311 0.7874939 0.6413734 0.7903485 0.6413734 0.7903485 0.6496351 0.7884368 0.6494074 0.7874939 0.6495311</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_18">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_18">0.0283361 0.9306569 0.2340767 0.9306569 0.2340767 0.9369777 0.0283361 0.9369777 0.0283361 0.9306569</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_18">0.0283361 0.9306569 0.2340767 0.9306569 0.2340767 0.9369777 0.0283361 0.9369777 0.0283361 0.9306569</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2402_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_4">0.6038104 0.5742092 0.6089175 0.5742092 0.6089175 0.5940338 0.6038104 0.5940338 0.6038104 0.5742092</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_4">0.6038104 0.5742092 0.6089175 0.5742092 0.6089175 0.5940338 0.6038104 0.5940338 0.6038104 0.5742092</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2422_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_4">0.954228 0.2019465 0.954228 0.2026932 0.9502889 0.2026932 0.9502889 0.2019465 0.954228 0.2019465</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_4">0.954228 0.2019465 0.954228 0.2026932 0.9502889 0.2026932 0.9502889 0.2019465 0.954228 0.2019465</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_8">0.5846626 0.5203859 0.5854251 0.5203785 0.5854251 0.5607472 0.5846626 0.5607697 0.5846626 0.5203859</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_8">0.5846626 0.5203859 0.5854251 0.5203785 0.5854251 0.5607472 0.5846626 0.5607697 0.5846626 0.5203859</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2450_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_0">0.6927985 0.5885518 0.6927853 0.5997567 0.6868588 0.5997567 0.686875 0.5885617 0.6927985 0.5885518</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_0">0.6927985 0.5885518 0.6927853 0.5997567 0.6868588 0.5997567 0.686875 0.5885617 0.6927985 0.5885518</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2411_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_3">0.6348606 0.5243309 0.6348606 0.5139257 0.6383208 0.5139257 0.6383208 0.5243309 0.6348606 0.5243309</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_3">0.6348606 0.5243309 0.6348606 0.5139257 0.6383208 0.5139257 0.6383208 0.5243309 0.6348606 0.5243309</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_2">0.621732 0.9939173 0.9299388 0.9939173 0.9299388 0.9990565 0.621732 0.9990565 0.621732 0.9939173</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_2">0.621732 0.9939173 0.9299388 0.9939173 0.9299388 0.9990565 0.621732 0.9990565 0.621732 0.9939173</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2465_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_0">0.6028495 0.4620345 0.608777 0.4620329 0.6087586 0.473236 0.6028334 0.473236 0.6028495 0.4620345</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_0">0.6028495 0.4620345 0.608777 0.4620329 0.6087586 0.473236 0.6028334 0.473236 0.6028495 0.4620345</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2423_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_4">0.6370298 0.6353102 0.6370298 0.5863747 0.6490642 0.5863747 0.6490642 0.6353102 0.6370298 0.6353102</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_4">0.6370298 0.6353102 0.6370298 0.5863747 0.6490642 0.5863747 0.6490642 0.6353102 0.6370298 0.6353102</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2420_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_3">0.6348301 0.5364963 0.6348301 0.5263351 0.6369986 0.5263351 0.6382719 0.5263351 0.6382719 0.5364963 0.6348301 0.5364963</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_3">0.6348301 0.5364963 0.6348301 0.5263351 0.6369986 0.5263351 0.6382719 0.5263351 0.6382719 0.5364963 0.6348301 0.5364963</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_0">0.5447904 0.6835417 0.5206465 0.683643 0.5206391 0.6807208 0.5146084 0.6808624 0.5146155 0.6836683 0.4903891 0.6837699 0.4903822 0.6812181 0.4843539 0.6813705 0.4843595 0.6837952 0.4602147 0.6838965 0.4602077 0.6810028 0.4541761 0.6811529 0.4541833 0.6839218 0.4299524 0.6840235 0.4299448 0.6811368 0.4239147 0.6812927 0.4239215 0.6840488 0.3997788 0.6841501 0.399771 0.6809216 0.3937375 0.6810753 0.3937455 0.6841754 0.3695123 0.6842771 0.3695042 0.6810485 0.3634718 0.6812071 0.3634801 0.6843024 0.3393373 0.6844037 0.3393286 0.6808362 0.3332963 0.6809948 0.3333049 0.684429 0.3090754 0.6845307 0.3090669 0.6809705 0.3030333 0.6811233 0.3030422 0.684556 0.2788994 0.6846573 0.2788898 0.6807619 0.2728584 0.6809112 0.2728681 0.6846826 0.2489945 0.6847829 0.2489845 0.6807355 0.2429498 0.6808834 0.2429611 0.6848081 0.2188186 0.6849095 0.2188076 0.6805128 0.2127754 0.6806715 0.2127864 0.6849347 0.1885567 0.6850364 0.188546 0.680655 0.1825134 0.6808003 0.1825248 0.6850617 0.1583813 0.685163 0.1583695 0.6804326 0.1523363 0.6805854 0.1523494 0.6851883 0.1281149 0.68529 0.1281036 0.6805677 0.1220727 0.6807171 0.1220841 0.6853153 0.0979396 0.6854166 0.097927 0.6803529 0.0918962 0.6805023 0.0919079 0.685442 0.0676792 0.6855436 0.0676653 0.6804897 0.0616331 0.6806341 0.0616462 0.6855689 0.0375024 0.6856702 0.0374878 0.6802702 0.0314581 0.6804255 0.0314709 0.6856955 0.0014315 0.6858215 0.5708793 0.4621856 0.5730374 0.6834232 0.5508229 0.6835164 0.5508162 0.6809329 0.5447844 0.6810827 0.5447904 0.6835417</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_0">0.5447904 0.6835417 0.5206465 0.683643 0.5206391 0.6807208 0.5146084 0.6808624 0.5146155 0.6836683 0.4903891 0.6837699 0.4903822 0.6812181 0.4843539 0.6813705 0.4843595 0.6837952 0.4602147 0.6838965 0.4602077 0.6810028 0.4541761 0.6811529 0.4541833 0.6839218 0.4299524 0.6840235 0.4299448 0.6811368 0.4239147 0.6812927 0.4239215 0.6840488 0.3997788 0.6841501 0.399771 0.6809216 0.3937375 0.6810753 0.3937455 0.6841754 0.3695123 0.6842771 0.3695042 0.6810485 0.3634718 0.6812071 0.3634801 0.6843024 0.3393373 0.6844037 0.3393286 0.6808362 0.3332963 0.6809948 0.3333049 0.684429 0.3090754 0.6845307 0.3090669 0.6809705 0.3030333 0.6811233 0.3030422 0.684556 0.2788994 0.6846573 0.2788898 0.6807619 0.2728584 0.6809112 0.2728681 0.6846826 0.2489945 0.6847829 0.2489845 0.6807355 0.2429498 0.6808834 0.2429611 0.6848081 0.2188186 0.6849095 0.2188076 0.6805128 0.2127754 0.6806715 0.2127864 0.6849347 0.1885567 0.6850364 0.188546 0.680655 0.1825134 0.6808003 0.1825248 0.6850617 0.1583813 0.685163 0.1583695 0.6804326 0.1523363 0.6805854 0.1523494 0.6851883 0.1281149 0.68529 0.1281036 0.6805677 0.1220727 0.6807171 0.1220841 0.6853153 0.0979396 0.6854166 0.097927 0.6803529 0.0918962 0.6805023 0.0919079 0.685442 0.0676792 0.6855436 0.0676653 0.6804897 0.0616331 0.6806341 0.0616462 0.6855689 0.0375024 0.6856702 0.0374878 0.6802702 0.0314581 0.6804255 0.0314709 0.6856955 0.0014315 0.6858215 0.5708793 0.4621856 0.5730374 0.6834232 0.5508229 0.6835164 0.5508162 0.6809329 0.5447844 0.6810827 0.5447904 0.6835417</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2413_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_2">0.5969712 0.5395584 0.6023367 0.5395584 0.6023367 0.5580434 0.5969712 0.5580434 0.5969712 0.5395584</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_2">0.5969712 0.5395584 0.6023367 0.5395584 0.6023367 0.5580434 0.5969712 0.5580434 0.5969712 0.5395584</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_1">0.6883416 0.6115927 0.6946054 0.6115927 0.6946054 0.611903 0.696884 0.611903 0.696884 0.6137227 0.6863703 0.6137227 0.6863703 0.6055732 0.6894922 0.6055732 0.6936409 0.6055732 0.6936409 0.6084253 0.6883416 0.6084253 0.6883416 0.6115927</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_1">0.6883416 0.6115927 0.6946054 0.6115927 0.6946054 0.611903 0.696884 0.611903 0.696884 0.6137227 0.6863703 0.6137227 0.6863703 0.6055732 0.6894922 0.6055732 0.6936409 0.6055732 0.6936409 0.6084253 0.6883416 0.6084253 0.6883416 0.6115927</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_12">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_12">0.8691772 0.9566005 0.6102963 0.9635037 0.6101612 0.9499716 0.6397544 0.9499716 0.6429002 0.9499716 0.8691397 0.9499716 0.8691772 0.9566005</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_12">0.8691772 0.9566005 0.6102963 0.9635037 0.6101612 0.9499716 0.6397544 0.9499716 0.6429002 0.9499716 0.8691397 0.9499716 0.8691772 0.9566005</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2427_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_2">0.6749511 0.5961071 0.6749511 0.6032715 0.6697606 0.6032715 0.6697606 0.5961071 0.6749511 0.5961071</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_2">0.6749511 0.5961071 0.6749511 0.6032715 0.6697606 0.6032715 0.6697606 0.5961071 0.6749511 0.5961071</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2464_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_2">0.937958 0.9657739 0.9412785 0.9657739 0.9412785 0.9751796 0.9379595 0.9751796 0.937958 0.9657739</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_2">0.937958 0.9657739 0.9412785 0.9657739 0.9412785 0.9751796 0.9379595 0.9751796 0.937958 0.9657739</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2461_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_4">0.6493353 0.6233672 0.6539069 0.6233672 0.6539069 0.6366063 0.6493378 0.6366063 0.6493353 0.6233672</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_4">0.6493353 0.6233672 0.6539069 0.6233672 0.6539069 0.6366063 0.6493378 0.6366063 0.6493353 0.6233672</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_5">0.6227982 0.5304136 0.6227982 0.5796496 0.6170005 0.5796496 0.6170005 0.5304136 0.6227982 0.5304136</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_5">0.6227982 0.5304136 0.6227982 0.5796496 0.6170005 0.5796496 0.6170005 0.5304136 0.6227982 0.5304136</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2407_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_3">0.6096727 0.5340135 0.6155114 0.5340135 0.6155114 0.5527325 0.6096727 0.5527325 0.6096727 0.5340135</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_3">0.6096727 0.5340135 0.6155114 0.5340135 0.6155114 0.5527325 0.6096727 0.5527325 0.6096727 0.5340135</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2459_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_1">0.6039048 0.4756691 0.6084512 0.4756691 0.6084512 0.4856948 0.6038104 0.4856948 0.6039563 0.4781005 0.6039048 0.4756691</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_1">0.6039048 0.4756691 0.6084512 0.4756691 0.6084512 0.4856948 0.6038104 0.4856948 0.6039563 0.4781005 0.6039048 0.4756691</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2425_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_2">0.8246214 0.6508516 0.8293648 0.6508516 0.8293648 0.6880867 0.8246214 0.6880867 0.8246214 0.6508516</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_2">0.8246214 0.6508516 0.8293648 0.6508516 0.8293648 0.6880867 0.8246214 0.6880867 0.8246214 0.6508516</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_8">0.8989349 0.9501216 0.8989349 0.9555994 0.8954913 0.9555994 0.894995 0.9555994 0.894995 0.9501216 0.8989349 0.9501216</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_8">0.8989349 0.9501216 0.8989349 0.9555994 0.8954913 0.9555994 0.894995 0.9555994 0.894995 0.9501216 0.8989349 0.9501216</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2405_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_3">0.6295848 0.5619787 0.6238397 0.5619787 0.6238397 0.5436276 0.6295848 0.5436276 0.6295848 0.5619787</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_3">0.6295848 0.5619787 0.6238397 0.5619787 0.6238397 0.5436276 0.6295848 0.5436276 0.6295848 0.5619787</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2415_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_1">0.8227992 0.678121 0.8227992 0.6981638 0.8182706 0.6981638 0.8182706 0.678121 0.8227992 0.678121</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_1">0.8227992 0.678121 0.8227992 0.6981638 0.8182706 0.6981638 0.8182706 0.678121 0.8227992 0.678121</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2402_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_2">0.6038104 0.6054977 0.6089175 0.6054977 0.6089175 0.62532 0.6038104 0.62532 0.6038104 0.6054977</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_2">0.6038104 0.6054977 0.6089175 0.6054977 0.6089175 0.62532 0.6038104 0.62532 0.6038104 0.6054977</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_6">0.6770501 0.7065095 0.6770501 0.7116789 0.6064701 0.7116789 0.5740107 0.7116789 0.5740107 0.7065095 0.6770501 0.7065095</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_6">0.6770501 0.7065095 0.6770501 0.7116789 0.6064701 0.7116789 0.5740107 0.7116789 0.5740107 0.7065095 0.6770501 0.7065095</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2418_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_1">0.6399 0.486618 0.6399 0.4754333 0.6467895 0.4754333 0.6468484 0.4754333 0.6468484 0.486618 0.6399 0.486618</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_1">0.6399 0.486618 0.6399 0.4754333 0.6467895 0.4754333 0.6468484 0.4754333 0.6468484 0.486618 0.6399 0.486618</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2414_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_1">0.6155444 0.6255174 0.6155444 0.636397 0.6096727 0.636397 0.6096727 0.6255174 0.6155444 0.6255174</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_1">0.6155444 0.6255174 0.6155444 0.636397 0.6096727 0.636397 0.6096727 0.6255174 0.6155444 0.6255174</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_7">0.494382 0.2124093 0.494382 0.2043796 0.5130785 0.2043796 0.5130785 0.2124093 0.494382 0.2124093</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_7">0.494382 0.2124093 0.494382 0.2043796 0.5130785 0.2043796 0.5130785 0.2124093 0.494382 0.2124093</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2404_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_2">0.6038104 0.5480649 0.609075 0.5480649 0.609075 0.5546555 0.6038104 0.5546555 0.6038104 0.5480649</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_2">0.6038104 0.5480649 0.609075 0.5480649 0.609075 0.5546555 0.6038104 0.5546555 0.6038104 0.5480649</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2464_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_1">0.9380084 0.9586375 0.9412785 0.9586375 0.9412785 0.9657739 0.937958 0.9657739 0.9380538 0.960785 0.9380084 0.9586375</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_1">0.9380084 0.9586375 0.9412785 0.9586375 0.9412785 0.9657739 0.937958 0.9657739 0.9380538 0.960785 0.9380084 0.9586375</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_0">0.9110278 0.9772729 0.9111894 0.9904639 0.8610609 0.9904401 0.8607342 0.9773498 0.8514208 0.977341 0.8517345 0.9900736 0.8021889 0.9900501 0.801843 0.9760125 0.7925252 0.9760021 0.7928778 0.9900457 0.7425189 0.9900219 0.7421661 0.9758354 0.7328524 0.9758289 0.7332027 0.9900175 0.7132847 0.990008 0.712941 0.9761838 0.7036258 0.9761706 0.7039776 0.9903657 0.6987441 0.9903632 0.6987622 0.9715528 0.6792511 0.9723821 0.6792704 0.981639 0.6955481 0.981235 0.6955844 0.9903618 0.6916633 0.9903599 0.6915325 0.9851141 0.6822189 0.9851053 0.6823483 0.9903555 0.6771253 0.990353 0.6770014 0.9673597 0.6193005 0.9672456 0.6193493 0.9763442 0.6101612 0.9763323 0.6101626 0.9653321 0.9362426 0.9653321 0.9362706 0.9914842 0.9205157 0.9914739 0.9203416 0.9772866 0.9110278 0.9772729</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_0">0.9110278 0.9772729 0.9111894 0.9904639 0.8610609 0.9904401 0.8607342 0.9773498 0.8514208 0.977341 0.8517345 0.9900736 0.8021889 0.9900501 0.801843 0.9760125 0.7925252 0.9760021 0.7928778 0.9900457 0.7425189 0.9900219 0.7421661 0.9758354 0.7328524 0.9758289 0.7332027 0.9900175 0.7132847 0.990008 0.712941 0.9761838 0.7036258 0.9761706 0.7039776 0.9903657 0.6987441 0.9903632 0.6987622 0.9715528 0.6792511 0.9723821 0.6792704 0.981639 0.6955481 0.981235 0.6955844 0.9903618 0.6916633 0.9903599 0.6915325 0.9851141 0.6822189 0.9851053 0.6823483 0.9903555 0.6771253 0.990353 0.6770014 0.9673597 0.6193005 0.9672456 0.6193493 0.9763442 0.6101612 0.9763323 0.6101626 0.9653321 0.9362426 0.9653321 0.9362706 0.9914842 0.9205157 0.9914739 0.9203416 0.9772866 0.9110278 0.9772729</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2450_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_2">0.6599903 0.5152317 0.66431 0.5152317 0.66431 0.5272928 0.6599921 0.5272928 0.6599903 0.5152317</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_2">0.6599903 0.5152317 0.66431 0.5152317 0.66431 0.5272928 0.6599921 0.5272928 0.6599903 0.5152317</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2423_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_2">0.7635564 0.6395433 0.7832286 0.6395433 0.7832286 0.6447688 0.7635564 0.6447688 0.7635564 0.6395433</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_2">0.7635564 0.6395433 0.7832286 0.6395433 0.7832286 0.6447688 0.7635564 0.6447688 0.7635564 0.6395433</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2409_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_3">0.5921499 0.5276511 0.5862237 0.5276511 0.5862237 0.515291 0.5921499 0.515291 0.5921499 0.5167656 0.5921499 0.5276511</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_3">0.5921499 0.5276511 0.5862237 0.5276511 0.5862237 0.515291 0.5921499 0.515291 0.5921499 0.5167656 0.5921499 0.5276511</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2457_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_4">0.6546533 0.5785127 0.6590113 0.5785127 0.6590113 0.5908537 0.6546554 0.5908537 0.6546533 0.5785127</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_4">0.6546533 0.5785127 0.6590113 0.5785127 0.6590113 0.5908537 0.6546554 0.5908537 0.6546533 0.5785127</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2410_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_3">0.6348467 0.5121654 0.6348467 0.5017673 0.6383 0.5017673 0.6383 0.5121654 0.6348467 0.5121654</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_3">0.6348467 0.5121654 0.6348467 0.5017673 0.6383 0.5017673 0.6383 0.5121654 0.6348467 0.5121654</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2409_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_4">0.5921499 0.515291 0.5862237 0.515291 0.5862237 0.4951338 0.5921499 0.4951338 0.5921499 0.5092432 0.5921499 0.515291</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_4">0.5921499 0.515291 0.5862237 0.515291 0.5862237 0.4951338 0.5921499 0.4951338 0.5921499 0.5092432 0.5921499 0.515291</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_23">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_23">0.5889717 0.9306569 0.5889717 0.9369777 0.5602909 0.9369777 0.5602909 0.9306569 0.562566 0.9306569 0.5630873 0.9306569 0.5633382 0.9306569 0.5642132 0.9306569 0.5668815 0.9306569 0.5799969 0.9306569 0.5889717 0.9306569</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_23">0.5889717 0.9306569 0.5889717 0.9369777 0.5602909 0.9369777 0.5602909 0.9306569 0.562566 0.9306569 0.5630873 0.9306569 0.5633382 0.9306569 0.5642132 0.9306569 0.5668815 0.9306569 0.5799969 0.9306569 0.5889717 0.9306569</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2410_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_1">0.6439326 0.5017673 0.6472958 0.5017673 0.6473838 0.5017673 0.6473838 0.5121654 0.6439326 0.5121654 0.6439326 0.5017673</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_1">0.6439326 0.5017673 0.6472958 0.5017673 0.6473838 0.5017673 0.6473838 0.5121654 0.6439326 0.5121654 0.6439326 0.5017673</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2424_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_1">0.5801944 0.5170317 0.5801944 0.5303904 0.5740107 0.5303904 0.5740107 0.5170317 0.5801944 0.5170317</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_1">0.5801944 0.5170317 0.5801944 0.5303904 0.5740107 0.5303904 0.5740107 0.5170317 0.5801944 0.5170317</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2462_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_4">0.7969609 0.6495351 0.7969609 0.6413734 0.8007212 0.6413734 0.8007212 0.6495311 0.7969609 0.6495351</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_4">0.7969609 0.6495351 0.7969609 0.6413734 0.8007212 0.6413734 0.8007212 0.6495311 0.7969609 0.6495351</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2459_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_4">0.6039025 0.5089276 0.6084512 0.5089276 0.6084512 0.5221354 0.6039048 0.5221354 0.6039025 0.5089276</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_4">0.6039025 0.5089276 0.6084512 0.5089276 0.6084512 0.5221354 0.6039048 0.5221354 0.6039025 0.5089276</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_1">0.8716724 0.9583066 0.8734275 0.9583066 0.8734275 0.9647202 0.8716724 0.9647202 0.8716724 0.9583066</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_1">0.8716724 0.9583066 0.8734275 0.9583066 0.8734275 0.9647202 0.8716724 0.9647202 0.8716724 0.9583066</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_6">0.6638984 0.5048662 0.6638984 0.472411 0.6646264 0.472411 0.6646264 0.5048662 0.6638984 0.5048662</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_6">0.6638984 0.5048662 0.6638984 0.472411 0.6646264 0.472411 0.6646264 0.5048662 0.6638984 0.5048662</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2413_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_0">0.6764623 0.5632603 0.6761114 0.5490489 0.6854375 0.5490549 0.6857926 0.5632603 0.6764623 0.5632603</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_0">0.6764623 0.5632603 0.6761114 0.5490489 0.6854375 0.5490549 0.6857926 0.5632603 0.6764623 0.5632603</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2466_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_3">0.6366436 0.5731744 0.6366913 0.5709221 0.6365435 0.5632204 0.641132 0.5632204 0.641132 0.5731744 0.6366436 0.5731744</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_3">0.6366436 0.5731744 0.6366913 0.5709221 0.6365435 0.5632204 0.641132 0.5632204 0.641132 0.5731744 0.6366436 0.5731744</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2451_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_2">0.6599922 0.571148 0.6599903 0.5590576 0.6643255 0.5590576 0.6643255 0.571148 0.6599922 0.571148</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_2">0.6599922 0.571148 0.6599903 0.5590576 0.6643255 0.5590576 0.6643255 0.571148 0.6599922 0.571148</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2422_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_3">0.4978883 0.2019465 0.9502889 0.2019465 0.9502889 0.2026932 0.4978883 0.2026932 0.4978883 0.2019465</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_3">0.4978883 0.2019465 0.9502889 0.2019465 0.9502889 0.2026932 0.4978883 0.2026932 0.4978883 0.2019465</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_8">0.6684021 0.472411 0.6684021 0.5048662 0.6676849 0.5048662 0.6676849 0.472411 0.6684021 0.472411</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_8">0.6684021 0.472411 0.6684021 0.5048662 0.6676849 0.5048662 0.6676849 0.472411 0.6684021 0.472411</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2467_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_3">0.5969733 0.4841909 0.6015657 0.4841909 0.6015657 0.4941612 0.5970618 0.4941612 0.5971158 0.4916084 0.5969733 0.4841909</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_3">0.5969733 0.4841909 0.6015657 0.4841909 0.6015657 0.4941612 0.5970618 0.4941612 0.5971158 0.4916084 0.5969733 0.4841909</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2458_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_1">0.622969 0.4890511 0.6274876 0.4890511 0.6274876 0.499052 0.6228627 0.499052 0.623014 0.4911754 0.622969 0.4890511</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_1">0.622969 0.4890511 0.6274876 0.4890511 0.6274876 0.499052 0.6228627 0.499052 0.623014 0.4911754 0.622969 0.4890511</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2424_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_0">0.5744992 0.459854 0.5807309 0.4599043 0.5807309 0.514064 0.5745019 0.5140098 0.5744992 0.459854</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_0">0.5744992 0.459854 0.5807309 0.4599043 0.5807309 0.514064 0.5745019 0.5140098 0.5744992 0.459854</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2418_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_3">0.631082 0.486618 0.631082 0.4754333 0.638029 0.4754333 0.638029 0.486618 0.631082 0.486618</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_3">0.631082 0.486618 0.631082 0.4754333 0.638029 0.4754333 0.638029 0.486618 0.631082 0.486618</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2465_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_3">0.6531996 0.4883507 0.6532466 0.4861297 0.6531525 0.4812264 0.6564715 0.4812264 0.6564715 0.4883507 0.6531996 0.4883507</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_3">0.6531996 0.4883507 0.6532466 0.4861297 0.6531525 0.4812264 0.6564715 0.4812264 0.6564715 0.4883507 0.6531996 0.4883507</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2423_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_1">0.7374147 0.6444461 0.737416 0.6447688 0.7122821 0.6444991 0.7122818 0.6444461 0.7374147 0.6444461</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_1">0.7374147 0.6444461 0.737416 0.6447688 0.7122821 0.6444991 0.7122818 0.6444461 0.7374147 0.6444461</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_35">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_35">0.9021012 0.9394053 0.9023983 0.9406869 0.9026956 0.9419686 0.9029927 0.9432502 0.9025278 0.9452555 0.8817782 0.9452555 0.8817782 0.9394042 0.9021012 0.9394053</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_35">0.9021012 0.9394053 0.9023983 0.9406869 0.9026956 0.9419686 0.9029927 0.9432502 0.9025278 0.9452555 0.8817782 0.9452555 0.8817782 0.9394042 0.9021012 0.9394053</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2460_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_3">0.6302712 0.5723542 0.6303315 0.5695055 0.6301926 0.5622764 0.6348706 0.5622764 0.6348706 0.5723542 0.6302712 0.5723542</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_3">0.6302712 0.5723542 0.6303315 0.5695055 0.6301926 0.5622764 0.6348706 0.5622764 0.6348706 0.5723542 0.6302712 0.5723542</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_1">0.5941491 0.6355736 0.59404 0.5608273 0.5967941 0.5608273 0.5967813 0.5689324 0.5967119 0.6124426 0.5966751 0.6355718 0.5941491 0.6355736</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_1">0.5941491 0.6355736 0.59404 0.5608273 0.5967941 0.5608273 0.5967813 0.5689324 0.5967119 0.6124426 0.5966751 0.6355718 0.5941491 0.6355736</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2419_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_4">0.6292135 0.5 0.6292135 0.4888206 0.6310781 0.4888206 0.6310781 0.5 0.6292135 0.5</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_4">0.6292135 0.5 0.6292135 0.4888206 0.6310781 0.4888206 0.6310781 0.5 0.6292135 0.5</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2467_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_1">0.5969712 0.4710525 0.5971158 0.4635218 0.5970639 0.4610705 0.6015657 0.4610705 0.6015657 0.4710525 0.5969712 0.4710525</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_1">0.5969712 0.4710525 0.5971158 0.4635218 0.5970639 0.4610705 0.6015657 0.4610705 0.6015657 0.4710525 0.5969712 0.4710525</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_27">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_27">0.358485 0.9731935 0.358485 0.9403893 0.3597967 0.9403893 0.3597967 0.9731935 0.358485 0.9731935</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_27">0.358485 0.9731935 0.358485 0.9403893 0.3597967 0.9403893 0.3597967 0.9731935 0.358485 0.9731935</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_12">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_12">0.7385287 0.7065095 0.7385287 0.7116789 0.7260497 0.7116789 0.7260497 0.7065095 0.7385287 0.7065095</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_12">0.7385287 0.7065095 0.7385287 0.7116789 0.7260497 0.7116789 0.7260497 0.7065095 0.7385287 0.7065095</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2464_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_0">0.6766169 0.4620402 0.6825432 0.4620318 0.6825253 0.473236 0.6765999 0.473236 0.6766169 0.4620402</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_0">0.6766169 0.4620402 0.6825432 0.4620318 0.6825253 0.473236 0.6765999 0.473236 0.6766169 0.4620402</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_25">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_25">0.3597967 0.9403893 0.5708335 0.9403893 0.5708335 0.9989182 0.3597967 0.9989182 0.3597967 0.9739908 0.5702289 0.9739908 0.5702289 0.9731935 0.3597967 0.9731935 0.3597967 0.9403893</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_25">0.3597967 0.9403893 0.5708335 0.9403893 0.5708335 0.9989182 0.3597967 0.9989182 0.3597967 0.9739908 0.5702289 0.9739908 0.5702289 0.9731935 0.3597967 0.9731935 0.3597967 0.9403893</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2466_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_2">0.6365413 0.5501032 0.641132 0.5501032 0.641132 0.5632204 0.6365435 0.5632204 0.6365413 0.5501032</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_2">0.6365413 0.5501032 0.641132 0.5501032 0.641132 0.5632204 0.6365435 0.5632204 0.6365413 0.5501032</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2403_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_2">0.9218428 0.9647202 0.9218428 0.9518815 0.9298956 0.9518815 0.9298956 0.9647202 0.9218428 0.9647202</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_2">0.9218428 0.9647202 0.9218428 0.9518815 0.9298956 0.9518815 0.9298956 0.9647202 0.9218428 0.9647202</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2414_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_3">0.6096727 0.5968539 0.6155444 0.5968539 0.6155444 0.5976237 0.6155444 0.6077393 0.6096727 0.6077393 0.6096727 0.5968539</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_3">0.6096727 0.5968539 0.6155444 0.5968539 0.6155444 0.5976237 0.6155444 0.6077393 0.6096727 0.6077393 0.6096727 0.5968539</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2424_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_3">0.5740107 0.577029 0.5801944 0.577029 0.5801944 0.5824541 0.5801944 0.5903934 0.5740107 0.5903934 0.5740107 0.577029</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_3">0.5740107 0.577029 0.5801944 0.577029 0.5801944 0.5824541 0.5801944 0.5903934 0.5740107 0.5903934 0.5740107 0.577029</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2408_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_2">0.6029397 0.6249335 0.5969712 0.6249335 0.5969712 0.6045877 0.6029397 0.6045877 0.6029397 0.6249335</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_2">0.6029397 0.6249335 0.5969712 0.6249335 0.5969712 0.6045877 0.6029397 0.6045877 0.6029397 0.6249335</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2403_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_4">0.9091353 0.9647202 0.9091353 0.9518815 0.9171864 0.9518815 0.9171864 0.9647202 0.9091353 0.9647202</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_4">0.9091353 0.9647202 0.9091353 0.9518815 0.9171864 0.9518815 0.9171864 0.9647202 0.9091353 0.9647202</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2420_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_0">0.6764609 0.5790755 0.6761114 0.5649139 0.6854047 0.5649199 0.6857584 0.5790755 0.6764609 0.5790755</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_0">0.6764609 0.5790755 0.6761114 0.5649139 0.6854047 0.5649199 0.6857584 0.5790755 0.6764609 0.5790755</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2426_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_4">0.7550558 0.6468558 0.7550558 0.6593674 0.7494548 0.6593674 0.7494548 0.6468558 0.7550558 0.6468558</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_4">0.7550558 0.6468558 0.7550558 0.6593674 0.7494548 0.6593674 0.7494548 0.6468558 0.7550558 0.6468558</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2452_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_2">0.6599903 0.602948 0.6643437 0.602948 0.6643437 0.6151535 0.6599923 0.6151535 0.6599903 0.602948</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_2">0.6599903 0.602948 0.6643437 0.602948 0.6643437 0.6151535 0.6599923 0.6151535 0.6599903 0.602948</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_9">0.7374003 0.6407601 0.7374147 0.6444461 0.7122818 0.6444461 0.7122619 0.6407601 0.7374003 0.6407601</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_9">0.7374003 0.6407601 0.7374147 0.6444461 0.7122818 0.6444461 0.7122619 0.6407601 0.7374003 0.6407601</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2412_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_3">0.641915 0.5542634 0.6460214 0.5542634 0.6460214 0.5543524 0.6460214 0.5629194 0.641915 0.5629194 0.641915 0.5542634</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_3">0.641915 0.5542634 0.6460214 0.5542634 0.6460214 0.5543524 0.6460214 0.5629194 0.641915 0.5629194 0.641915 0.5542634</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2418_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_2">0.638029 0.486618 0.638029 0.4754333 0.6399 0.4754333 0.6399 0.486618 0.638029 0.486618</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_2">0.638029 0.486618 0.638029 0.4754333 0.6399 0.4754333 0.6399 0.486618 0.638029 0.486618</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2405_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_0">0.59404 0.5325511 0.59404 0.510818 0.5968503 0.5111136 0.5968503 0.5328467 0.59404 0.5325511</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_0">0.59404 0.5325511 0.59404 0.510818 0.5968503 0.5111136 0.5968503 0.5328467 0.59404 0.5325511</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2453_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_4">0.6648895 0.6242596 0.66922 0.6242596 0.66922 0.6364001 0.6648915 0.6364001 0.6648895 0.6242596</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_4">0.6648895 0.6242596 0.66922 0.6242596 0.66922 0.6364001 0.6648915 0.6364001 0.6648895 0.6242596</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2460_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_2">0.6301906 0.549004 0.6348706 0.549004 0.6348706 0.5622764 0.6301926 0.5622764 0.6301906 0.549004</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_2">0.6301906 0.549004 0.6348706 0.549004 0.6348706 0.5622764 0.6301926 0.5622764 0.6301906 0.549004</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2458_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_0">0.6761288 0.4887617 0.682072 0.4887536 0.6820593 0.5 0.6761114 0.5 0.6761288 0.4887617</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_0">0.6761288 0.4887617 0.682072 0.4887536 0.6820593 0.5 0.6761114 0.5 0.6761288 0.4887617</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2407_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_0">0.59404 0.5082143 0.59404 0.4864803 0.5968503 0.4867818 0.5968503 0.5085158 0.59404 0.5082143</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_0">0.59404 0.5082143 0.59404 0.4864803 0.5968503 0.4867818 0.5968503 0.5085158 0.59404 0.5082143</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_8">0.5838093 0.4846483 0.5813385 0.4846483 0.5813385 0.459854 0.5838093 0.459854 0.5838093 0.4846483</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_8">0.5838093 0.4846483 0.5813385 0.4846483 0.5813385 0.459854 0.5838093 0.459854 0.5838093 0.4846483</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_14">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_14">0.5901319 0.9364939 0.5933177 0.9364863 0.5964523 0.9364789 0.7928717 0.9360113 0.7928758 0.9367397 0.5901352 0.9367397 0.5901319 0.9364939</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_14">0.5901319 0.9364939 0.5933177 0.9364863 0.5964523 0.9364789 0.7928717 0.9360113 0.7928758 0.9367397 0.5901352 0.9367397 0.5901319 0.9364939</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2460_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_1">0.6302736 0.5389295 0.6348706 0.5389295 0.6348706 0.549004 0.6301906 0.549004 0.6303315 0.541665 0.6302736 0.5389295</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_1">0.6302736 0.5389295 0.6348706 0.5389295 0.6348706 0.549004 0.6301906 0.549004 0.6303315 0.541665 0.6302736 0.5389295</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2451_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_4">0.6600044 0.5803206 0.6643255 0.5803206 0.6643255 0.592409 0.6600065 0.592409 0.6600044 0.5803206</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_4">0.6600044 0.5803206 0.6643255 0.5803206 0.6643255 0.592409 0.6600065 0.592409 0.6600044 0.5803206</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_4">0.0025013 0.2747388 0.0024573 0.3284543 0.0004885 0.3284671 0.0005366 0.2698362 0.5557551 0.2698362 0.5557011 0.3248324 0.5537324 0.3248453 0.5537816 0.2747388 0.0025013 0.2747388</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_4">0.0025013 0.2747388 0.0024573 0.3284543 0.0004885 0.3284671 0.0005366 0.2698362 0.5557551 0.2698362 0.5557011 0.3248324 0.5537324 0.3248453 0.5537816 0.2747388 0.0025013 0.2747388</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2452_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_4">0.6600156 0.6244226 0.6643437 0.6244226 0.6643437 0.6366303 0.6600177 0.6366303 0.6600156 0.6244226</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_4">0.6600156 0.6244226 0.6643437 0.6244226 0.6643437 0.6366303 0.6600177 0.6366303 0.6600156 0.6244226</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2462_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_2">0.7903485 0.6496351 0.7903485 0.6413734 0.7941079 0.6413734 0.7941079 0.6496311 0.7903485 0.6496351</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_2">0.7903485 0.6496351 0.7903485 0.6413734 0.7941079 0.6413734 0.7941079 0.6496311 0.7903485 0.6496351</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2466_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_4">0.6366436 0.5731744 0.641132 0.5731744 0.641132 0.5862881 0.6366459 0.5862881 0.6366436 0.5731744</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_4">0.6366436 0.5731744 0.641132 0.5731744 0.641132 0.5862881 0.6366459 0.5862881 0.6366436 0.5731744</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_2">0.8514532 0.9257908 0.8514532 0.9309833 0.8357943 0.9309833 0.8357943 0.9257908 0.8514532 0.9257908</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_2">0.8514532 0.9257908 0.8514532 0.9309833 0.8357943 0.9309833 0.8357943 0.9257908 0.8514532 0.9257908</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2413_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_1">0.6023367 0.5580434 0.6023367 0.5684965 0.6023367 0.5693717 0.5969712 0.5693717 0.5969712 0.5580434 0.6023367 0.5580434</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_1">0.6023367 0.5580434 0.6023367 0.5684965 0.6023367 0.5693717 0.5969712 0.5693717 0.5969712 0.5580434 0.6023367 0.5580434</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2461_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_3">0.6493353 0.6233672 0.6493891 0.6208258 0.6492449 0.6133145 0.6539069 0.6133145 0.6539069 0.6233672 0.6493353 0.6233672</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_3">0.6493353 0.6233672 0.6493891 0.6208258 0.6492449 0.6133145 0.6539069 0.6133145 0.6539069 0.6233672 0.6493353 0.6233672</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2425_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_0">0.6497313 0.5437956 0.6545767 0.5437974 0.6545767 0.5873642 0.6497321 0.5873545 0.6497313 0.5437956</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_0">0.6497313 0.5437956 0.6545767 0.5437974 0.6545767 0.5873642 0.6497321 0.5873545 0.6497313 0.5437956</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2453_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_2">0.6648754 0.6028931 0.66922 0.6028931 0.66922 0.6150365 0.6648773 0.6150365 0.6648754 0.6028931</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_2">0.6648754 0.6028931 0.66922 0.6028931 0.66922 0.6150365 0.6648773 0.6150365 0.6648754 0.6028931</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_28">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_28">0.2584392 0.9403893 0.2584392 0.9731935 0.2572964 0.9731935 0.2572964 0.9403893 0.2584392 0.9403893</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_28">0.2584392 0.9403893 0.2584392 0.9731935 0.2572964 0.9731935 0.2572964 0.9403893 0.2584392 0.9403893</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2455_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_1">0.6492817 0.5 0.6536481 0.5 0.6536481 0.5094099 0.6492428 0.5094099 0.649356 0.5035141 0.6492817 0.5</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_1">0.6492817 0.5 0.6536481 0.5 0.6536481 0.5094099 0.6492428 0.5094099 0.649356 0.5035141 0.6492817 0.5</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2415_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_4">0.8182706 0.6447688 0.8227992 0.6447688 0.8227992 0.6514274 0.8182706 0.6514274 0.8182706 0.6447688</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_4">0.8182706 0.6447688 0.8227992 0.6447688 0.8227992 0.6514274 0.8182706 0.6514274 0.8182706 0.6447688</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_6">0.9539796 0.2043796 0.9539796 0.2124093 0.5130785 0.2124093 0.5130785 0.2043796 0.9539796 0.2043796</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_6">0.9539796 0.2043796 0.9539796 0.2124093 0.5130785 0.2124093 0.5130785 0.2043796 0.9539796 0.2043796</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_5">0.9715171 0.2043796 0.9715171 0.2051095 0.9715171 0.2124093 0.9539796 0.2124093 0.9539796 0.2043796 0.9715171 0.2043796</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_5">0.9715171 0.2043796 0.9715171 0.2051095 0.9715171 0.2124093 0.9539796 0.2124093 0.9539796 0.2043796 0.9715171 0.2043796</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2417_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_1">0.639918 0.462033 0.6461561 0.462033 0.6468778 0.462033 0.6468778 0.473236 0.639918 0.473236 0.639918 0.462033</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_1">0.639918 0.462033 0.6461561 0.462033 0.6468778 0.462033 0.6468778 0.473236 0.639918 0.473236 0.639918 0.462033</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2408_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_4">0.6029397 0.5921208 0.5969712 0.5921208 0.5969712 0.5717762 0.6029397 0.5717762 0.6029397 0.5921208</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_4">0.6029397 0.5921208 0.5969712 0.5921208 0.5969712 0.5717762 0.6029397 0.5717762 0.6029397 0.5921208</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2453_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_0">0.6863871 0.5350255 0.6923115 0.5350207 0.6922961 0.5462287 0.6863703 0.5462287 0.6863871 0.5350255</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_0">0.6863871 0.5350255 0.6923115 0.5350207 0.6922961 0.5462287 0.6863703 0.5462287 0.6863871 0.5350255</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2417_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_0">0.665364 0.5593539 0.665364 0.5302743 0.6685015 0.5305386 0.6685015 0.5596107 0.665364 0.5593539</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_0">0.665364 0.5593539 0.665364 0.5302743 0.6685015 0.5305386 0.6685015 0.5596107 0.665364 0.5593539</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2404_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_3">0.6038104 0.5309215 0.609075 0.5309215 0.609075 0.5316101 0.609075 0.5480649 0.6038104 0.5480649 0.6038104 0.5309215</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_3">0.6038104 0.5309215 0.609075 0.5309215 0.609075 0.5316101 0.609075 0.5480649 0.6038104 0.5480649 0.6038104 0.5309215</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2450_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_1">0.6599954 0.5060828 0.66431 0.5060828 0.66431 0.5152317 0.6599903 0.5152317 0.6600847 0.5103077 0.6599954 0.5060828</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_1">0.6599954 0.5060828 0.66431 0.5060828 0.66431 0.5152317 0.6599903 0.5152317 0.6600847 0.5103077 0.6599954 0.5060828</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2453_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_3">0.6648895 0.6242596 0.6649758 0.620172 0.6648773 0.6150365 0.66922 0.6150365 0.66922 0.6242596 0.6648895 0.6242596</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_3">0.6648895 0.6242596 0.6649758 0.620172 0.6648773 0.6150365 0.66922 0.6150365 0.66922 0.6242596 0.6648895 0.6242596</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_3">0.9123909 0.9454246 0.9124159 0.9475712 0.8363851 0.9476886 0.8363459 0.9454246 0.893011 0.9454246 0.89582 0.9454246 0.9123909 0.9454246</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_3">0.9123909 0.9454246 0.9124159 0.9475712 0.8363851 0.9476886 0.8363459 0.9454246 0.893011 0.9454246 0.89582 0.9454246 0.9123909 0.9454246</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2419_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_1">0.6398778 0.4888206 0.646449 0.4888206 0.6468108 0.4888206 0.6468108 0.5 0.6398778 0.5 0.6398778 0.4888206</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_1">0.6398778 0.4888206 0.646449 0.4888206 0.6468108 0.4888206 0.6468108 0.5 0.6398778 0.5 0.6398778 0.4888206</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_6">0.7647746 0.747611 0.764784 0.749319 0.751609 0.7492756 0.7515895 0.7484775 0.7423055 0.748453 0.7423249 0.749245 0.6919948 0.7490792 0.6827101 0.7490486 0.6830171 0.761506 0.6336392 0.7615293 0.633606 0.7483445 0.6239176 0.7483416 0.6239527 0.7615339 0.5745336 0.7615572 0.5744992 0.747611 0.7647746 0.747611</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_6">0.7647746 0.747611 0.764784 0.749319 0.751609 0.7492756 0.7515895 0.7484775 0.7423055 0.748453 0.7423249 0.749245 0.6919948 0.7490792 0.6827101 0.7490486 0.6830171 0.761506 0.6336392 0.7615293 0.633606 0.7483445 0.6239176 0.7483416 0.6239527 0.7615339 0.5745336 0.7615572 0.5744992 0.747611 0.7647746 0.747611</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2416_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_3">0.6546165 0.5971379 0.659127 0.5971379 0.659127 0.6145476 0.6546165 0.6145476 0.6546165 0.5971379</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_3">0.6546165 0.5971379 0.659127 0.5971379 0.659127 0.6145476 0.6546165 0.6145476 0.6546165 0.5971379</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_10">0.8734275 0.9583066 0.8808411 0.9583066 0.8808411 0.9647202 0.8734275 0.9647202 0.8734275 0.9583066</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_10">0.8734275 0.9583066 0.8808411 0.9583066 0.8808411 0.9647202 0.8734275 0.9647202 0.8734275 0.9583066</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2459_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_3">0.6039025 0.5089276 0.6039563 0.5063866 0.6038125 0.4988987 0.6084512 0.4988987 0.6084512 0.5089276 0.6039025 0.5089276</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_3">0.6039025 0.5089276 0.6039563 0.5063866 0.6038125 0.4988987 0.6084512 0.4988987 0.6084512 0.5089276 0.6039025 0.5089276</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_4">0.9600896 0.9257908 0.9600896 0.9309833 0.8569222 0.9309833 0.8569222 0.9257908 0.9600896 0.9257908</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_4">0.9600896 0.9257908 0.9600896 0.9309833 0.8569222 0.9309833 0.8569222 0.9257908 0.9600896 0.9257908</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_32">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_32">0.358485 0.9797044 0.358485 0.9739908 0.3597967 0.9739908 0.3597967 0.9989182 0.3317179 0.9989182 0.3317179 0.9925402 0.3317179 0.9821227 0.3425884 0.9821227 0.3436552 0.9821227 0.3515489 0.9821227 0.3539346 0.9821227 0.3571859 0.9821227 0.3571859 0.9954103 0.3584667 0.9954103 0.3584667 0.9797044 0.358485 0.9797044</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_32">0.358485 0.9797044 0.358485 0.9739908 0.3597967 0.9739908 0.3597967 0.9989182 0.3317179 0.9989182 0.3317179 0.9925402 0.3317179 0.9821227 0.3425884 0.9821227 0.3436552 0.9821227 0.3515489 0.9821227 0.3539346 0.9821227 0.3571859 0.9821227 0.3571859 0.9954103 0.3584667 0.9954103 0.3584667 0.9797044 0.358485 0.9797044</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_4">0.9379584 0.9557261 0.937958 0.9549878 0.9402671 0.9549878 0.9402688 0.9557261 0.9379584 0.9557261</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_4">0.9379584 0.9557261 0.937958 0.9549878 0.9402671 0.9549878 0.9402688 0.9557261 0.9379584 0.9557261</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2456_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_1">0.6546439 0.5024331 0.6589954 0.5024331 0.6589954 0.5117566 0.6546165 0.5117566 0.6547233 0.5061913 0.6546439 0.5024331</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_1">0.6546439 0.5024331 0.6589954 0.5024331 0.6589954 0.5117566 0.6546165 0.5117566 0.6547233 0.5061913 0.6546439 0.5024331</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2463_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_4">0.7970338 0.6592244 0.7970338 0.651085 0.8008218 0.651085 0.8008218 0.65922 0.7970338 0.6592244</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_4">0.7970338 0.6592244 0.7970338 0.651085 0.8008218 0.651085 0.8008218 0.65922 0.7970338 0.6592244</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2458_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_4">0.6229666 0.5222413 0.6274876 0.5222413 0.6274876 0.5354118 0.622969 0.5354118 0.6229666 0.5222413</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_4">0.6229666 0.5222413 0.6274876 0.5222413 0.6274876 0.5354118 0.622969 0.5354118 0.6229666 0.5222413</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2411_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_0">0.6857582 0.5158151 0.6764621 0.5158151 0.6761114 0.5016572 0.6854051 0.5016593 0.6857582 0.5158151</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_0">0.6857582 0.5158151 0.6764621 0.5158151 0.6761114 0.5016572 0.6854051 0.5016593 0.6857582 0.5158151</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2419_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_0">0.660481 0.503387 0.6604788 0.4743149 0.6636186 0.4745808 0.6636186 0.5036497 0.660481 0.503387</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_0">0.660481 0.503387 0.6604788 0.4743149 0.6636186 0.4745808 0.6636186 0.5036497 0.660481 0.503387</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2461_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_2">0.6492428 0.6000712 0.6539069 0.6000712 0.6539069 0.6133145 0.6492449 0.6133145 0.6492428 0.6000712</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_2">0.6492428 0.6000712 0.6539069 0.6000712 0.6539069 0.6133145 0.6492449 0.6133145 0.6492428 0.6000712</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2420_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_1">0.6438859 0.5263351 0.645158 0.5263351 0.6473263 0.5263351 0.6473263 0.5364963 0.6438859 0.5364963 0.6438859 0.5263351</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_1">0.6438859 0.5263351 0.645158 0.5263351 0.6473263 0.5263351 0.6473263 0.5364963 0.6438859 0.5364963 0.6438859 0.5263351</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2413_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_3">0.5969712 0.5282258 0.6023367 0.5282258 0.6023367 0.5283095 0.6023367 0.5293915 0.6023367 0.5395584 0.5969712 0.5395584 0.5969712 0.5282258</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_3">0.5969712 0.5282258 0.6023367 0.5282258 0.6023367 0.5283095 0.6023367 0.5293915 0.6023367 0.5395584 0.5969712 0.5395584 0.5969712 0.5282258</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2426_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_1">0.6235585 0.6355395 0.6170005 0.6356386 0.6170005 0.5815253 0.6235594 0.5815085 0.6235585 0.6355395</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_1">0.6235585 0.6355395 0.6170005 0.6356386 0.6170005 0.5815253 0.6235594 0.5815085 0.6235585 0.6355395</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2407_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_1">0.6155114 0.5587654 0.6155114 0.5774844 0.6096727 0.5774844 0.6096727 0.5587654 0.6155114 0.5587654</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_1">0.6155114 0.5587654 0.6155114 0.5774844 0.6096727 0.5774844 0.6096727 0.5587654 0.6155114 0.5587654</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_7">0.6835856 0.7065095 0.6835856 0.7116789 0.6771211 0.7116789 0.6770501 0.7116789 0.6770501 0.7065095 0.6835856 0.7065095</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_7">0.6835856 0.7065095 0.6835856 0.7116789 0.6771211 0.7116789 0.6770501 0.7116789 0.6770501 0.7065095 0.6835856 0.7065095</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_3">0.93796 0.9564645 0.9379584 0.9557261 0.9402688 0.9557261 0.9402729 0.9564645 0.93796 0.9564645</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_3">0.93796 0.9564645 0.9379584 0.9557261 0.9402688 0.9557261 0.9402729 0.9564645 0.93796 0.9564645</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_8">0.7974733 0.7635586 0.7974925 0.7871046 0.5745209 0.7856449 0.5744992 0.7635586 0.7974733 0.7635586</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_8">0.7974733 0.7635586 0.7974925 0.7871046 0.5745209 0.7856449 0.5744992 0.7635586 0.7974733 0.7635586</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_5">0.696884 0.6137227 0.696884 0.6213683 0.6863703 0.6213683 0.6863703 0.6137227 0.696884 0.6137227</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_5">0.696884 0.6137227 0.696884 0.6213683 0.6863703 0.6213683 0.6863703 0.6137227 0.696884 0.6137227</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2408_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_0">0.6697614 0.5020945 0.6754469 0.5012165 0.6754469 0.5243677 0.6697606 0.5252444 0.6697614 0.5020945</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_0">0.6697614 0.5020945 0.6754469 0.5012165 0.6754469 0.5243677 0.6697606 0.5252444 0.6697614 0.5020945</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2411_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_1">0.6439665 0.5243309 0.6439665 0.5139257 0.6474263 0.5139257 0.6474263 0.5243309 0.6439665 0.5243309</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_1">0.6439665 0.5243309 0.6439665 0.5139257 0.6474263 0.5139257 0.6474263 0.5243309 0.6439665 0.5243309</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_6">0.582375 0.5608048 0.5816126 0.5607648 0.5816126 0.4887406 0.582375 0.4887669 0.582375 0.5608048</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_6">0.582375 0.5608048 0.5816126 0.5607648 0.5816126 0.4887406 0.582375 0.4887669 0.582375 0.5608048</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2404_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_4">0.6038104 0.5243309 0.609075 0.5243309 0.609075 0.5309215 0.6038104 0.5309215 0.6038104 0.5243309</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_4">0.6038104 0.5243309 0.609075 0.5243309 0.609075 0.5309215 0.6038104 0.5309215 0.6038104 0.5243309</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2453_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_1">0.6648915 0.5936739 0.66922 0.5936739 0.66922 0.6028931 0.6648754 0.6028931 0.6649758 0.5976614 0.6648915 0.5936739</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2453_1">0.6648915 0.5936739 0.66922 0.5936739 0.66922 0.6028931 0.6648754 0.6028931 0.6649758 0.5976614 0.6648915 0.5936739</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2460_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_0">0.6879528 0.4708426 0.6879543 0.4856551 0.6834392 0.4856109 0.6834392 0.4708029 0.6879528 0.4708426</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2460_0">0.6879528 0.4708426 0.6879543 0.4856551 0.6834392 0.4856109 0.6834392 0.4708029 0.6879528 0.4708426</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2426_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_0">0.8031551 0.6545012 0.8031265 0.6381702 0.8173839 0.6381717 0.8173925 0.6545012 0.8031551 0.6545012</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_0">0.8031551 0.6545012 0.8031265 0.6381702 0.8173839 0.6381717 0.8173925 0.6545012 0.8031551 0.6545012</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_15">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_15">0.5901319 0.9312981 0.646246 0.9312981 0.6493806 0.9312981 0.7865778 0.9312981 0.7865921 0.9338389 0.5901728 0.9343066 0.5901319 0.9312981</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_15">0.5901319 0.9312981 0.646246 0.9312981 0.6493806 0.9312981 0.7865778 0.9312981 0.7865921 0.9338389 0.5901728 0.9343066 0.5901319 0.9312981</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2419_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_3">0.6310781 0.5 0.6310781 0.4888206 0.6380119 0.4888206 0.6380119 0.5 0.6310781 0.5</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2419_3">0.6310781 0.5 0.6310781 0.4888206 0.6380119 0.4888206 0.6380119 0.5 0.6310781 0.5</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2454_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_2">0.6492428 0.4718121 0.6525582 0.4718121 0.6525582 0.4811511 0.6492444 0.4811511 0.6492428 0.4718121</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_2">0.6492428 0.4718121 0.6525582 0.4718121 0.6525582 0.4811511 0.6492444 0.4811511 0.6492428 0.4718121</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_20">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_20">0.2619039 0.9306569 0.3112145 0.9306569 0.3112145 0.9369777 0.2619039 0.9369777 0.2619039 0.9306569</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_20">0.2619039 0.9306569 0.3112145 0.9306569 0.3112145 0.9369777 0.2619039 0.9369777 0.2619039 0.9306569</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2405_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_1">0.6295848 0.5862445 0.6238397 0.5862445 0.6238397 0.5678934 0.6295848 0.5678934 0.6295848 0.5862445</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_1">0.6295848 0.5862445 0.6238397 0.5862445 0.6238397 0.5678934 0.6295848 0.5678934 0.6295848 0.5862445</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_10">0.7173988 0.7065095 0.7173988 0.7116789 0.7034673 0.7116789 0.7034673 0.7065095 0.7173988 0.7065095</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_10">0.7173988 0.7065095 0.7173988 0.7116789 0.7034673 0.7116789 0.7034673 0.7065095 0.7173988 0.7065095</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2461_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_0">0.6863873 0.57514 0.6923323 0.5751334 0.6923171 0.5863747 0.6863703 0.5863747 0.6863873 0.57514</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_0">0.6863873 0.57514 0.6923323 0.5751334 0.6923171 0.5863747 0.6863703 0.5863747 0.6863873 0.57514</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2451_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_3">0.6600044 0.5803206 0.6600903 0.5762593 0.6599922 0.571148 0.6643255 0.571148 0.6643255 0.5803206 0.6600044 0.5803206</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_3">0.6600044 0.5803206 0.6600903 0.5762593 0.6599922 0.571148 0.6643255 0.571148 0.6643255 0.5803206 0.6600044 0.5803206</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_0">0.5831376 0.5203983 0.5839 0.5203958 0.5839 0.5608097 0.5831376 0.5608273 0.5831376 0.5203983</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_0">0.5831376 0.5203983 0.5839 0.5203958 0.5839 0.5608097 0.5831376 0.5608273 0.5831376 0.5203983</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2418_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_4">0.6292135 0.486618 0.6292135 0.4754333 0.631082 0.4754333 0.631082 0.486618 0.6292135 0.486618</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_4">0.6292135 0.486618 0.6292135 0.4754333 0.631082 0.4754333 0.631082 0.486618 0.6292135 0.486618</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2427_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_4">0.6749511 0.6166476 0.6749511 0.6238121 0.6697606 0.6238121 0.6697606 0.6166476 0.6749511 0.6166476</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_4">0.6749511 0.6166476 0.6749511 0.6238121 0.6697606 0.6238121 0.6697606 0.6166476 0.6749511 0.6166476</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_3">0.8343918 0.940972 0.8392796 0.9410331 0.8762314 0.9414946 0.8813527 0.9415586 0.8813531 0.9440389 0.8343923 0.9440389 0.8343918 0.940972</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_3">0.8343918 0.940972 0.8392796 0.9410331 0.8762314 0.9414946 0.8813527 0.9415586 0.8813531 0.9440389 0.8343923 0.9440389 0.8343918 0.940972</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2407_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_2">0.6096727 0.5527325 0.6155114 0.5527325 0.6155114 0.5587654 0.6096727 0.5587654 0.6096727 0.5527325</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_2">0.6096727 0.5527325 0.6155114 0.5527325 0.6155114 0.5587654 0.6096727 0.5587654 0.6096727 0.5527325</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_3">0.5730374 0.6834232 0.570735 0.9294404 0 0.9294404 0.5730374 0.6834232</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_3">0.5730374 0.6834232 0.570735 0.9294404 0 0.9294404 0.5730374 0.6834232</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_2">0.1523494 0.6851883 0.152369 0.6920031 0.158398 0.6918565 0.1583813 0.685163 0.1825248 0.6850617 0.1825441 0.6922207 0.188574 0.6920656 0.1885567 0.6850364 0.2127864 0.6849347 0.2128049 0.692086 0.2188361 0.6919368 0.2188186 0.6849095 0.2429611 0.6848081 0.2429826 0.6923012 0.2490127 0.692146 0.2489945 0.6847829 0.2728681 0.6846826 0.2728879 0.6923276 0.278918 0.6921717 0.2788994 0.6846573 0.3030422 0.684556 0.3030629 0.6925397 0.3090941 0.6923887 0.3090754 0.6845307 0.3333049 0.684429 0.3333247 0.6924053 0.3393562 0.6922554 0.3393373 0.6844037 0.3634801 0.6843024 0.3635025 0.6926275 0.3695328 0.6924725 0.3695123 0.6842771 0.3937455 0.6841754 0.393767 0.6924908 0.3997986 0.6923409 0.3997788 0.6841501 0.4239215 0.6840488 0.4239429 0.6927024 0.4299749 0.6925533 0.4299524 0.6840235 0.4541833 0.6839218 0.4542057 0.6925684 0.4602354 0.692422 0.4602147 0.6838965 0.4843595 0.6837952 0.4843802 0.6927837 0.4904134 0.6926395 0.4903891 0.6837699 0.5146155 0.6836683 0.5146372 0.6922865 0.5206679 0.6921316 0.5206465 0.683643 0.5447904 0.6835417 0.5448123 0.692502 0.5508459 0.6923484 0.5508229 0.6835164 0.5730374 0.6834232 0 0.9294404 0.0014315 0.6858215 0.0314709 0.6856955 0.0314859 0.6918359 0.0375186 0.6916906 0.0375024 0.6856702 0.0616462 0.6855689 0.0616634 0.6920553 0.0676966 0.6919025 0.0676792 0.6855436 0.0919079 0.685442 0.0919232 0.6919214 0.0979554 0.6917769 0.0979396 0.6854166 0.1220841 0.6853153 0.1221011 0.6921409 0.128131 0.6919857 0.1281149 0.68529 0.1523494 0.6851883</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_2">0.1523494 0.6851883 0.152369 0.6920031 0.158398 0.6918565 0.1583813 0.685163 0.1825248 0.6850617 0.1825441 0.6922207 0.188574 0.6920656 0.1885567 0.6850364 0.2127864 0.6849347 0.2128049 0.692086 0.2188361 0.6919368 0.2188186 0.6849095 0.2429611 0.6848081 0.2429826 0.6923012 0.2490127 0.692146 0.2489945 0.6847829 0.2728681 0.6846826 0.2728879 0.6923276 0.278918 0.6921717 0.2788994 0.6846573 0.3030422 0.684556 0.3030629 0.6925397 0.3090941 0.6923887 0.3090754 0.6845307 0.3333049 0.684429 0.3333247 0.6924053 0.3393562 0.6922554 0.3393373 0.6844037 0.3634801 0.6843024 0.3635025 0.6926275 0.3695328 0.6924725 0.3695123 0.6842771 0.3937455 0.6841754 0.393767 0.6924908 0.3997986 0.6923409 0.3997788 0.6841501 0.4239215 0.6840488 0.4239429 0.6927024 0.4299749 0.6925533 0.4299524 0.6840235 0.4541833 0.6839218 0.4542057 0.6925684 0.4602354 0.692422 0.4602147 0.6838965 0.4843595 0.6837952 0.4843802 0.6927837 0.4904134 0.6926395 0.4903891 0.6837699 0.5146155 0.6836683 0.5146372 0.6922865 0.5206679 0.6921316 0.5206465 0.683643 0.5447904 0.6835417 0.5448123 0.692502 0.5508459 0.6923484 0.5508229 0.6835164 0.5730374 0.6834232 0 0.9294404 0.0014315 0.6858215 0.0314709 0.6856955 0.0314859 0.6918359 0.0375186 0.6916906 0.0375024 0.6856702 0.0616462 0.6855689 0.0616634 0.6920553 0.0676966 0.6919025 0.0676792 0.6855436 0.0919079 0.685442 0.0919232 0.6919214 0.0979554 0.6917769 0.0979396 0.6854166 0.1220841 0.6853153 0.1221011 0.6921409 0.128131 0.6919857 0.1281149 0.68529 0.1523494 0.6851883</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_19">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_19">0.2619039 0.9369777 0.2340767 0.9369777 0.2340767 0.9306569 0.2375872 0.9306569 0.2375872 0.9366617 0.2400055 0.9366617 0.2400055 0.9306569 0.2404204 0.9306569 0.2539417 0.9306569 0.2587812 0.9306569 0.2619039 0.9306569 0.2619039 0.9369777</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_19">0.2619039 0.9369777 0.2340767 0.9369777 0.2340767 0.9306569 0.2375872 0.9306569 0.2375872 0.9366617 0.2400055 0.9366617 0.2400055 0.9306569 0.2404204 0.9306569 0.2539417 0.9306569 0.2587812 0.9306569 0.2619039 0.9306569 0.2619039 0.9369777</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2413_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_4">0.5969712 0.5097324 0.6023367 0.5097324 0.6023367 0.5282258 0.5969712 0.5282258 0.5969712 0.5097324</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2413_4">0.5969712 0.5097324 0.6023367 0.5097324 0.6023367 0.5282258 0.5969712 0.5282258 0.5969712 0.5097324</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2405_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_4">0.6295848 0.5436276 0.6238397 0.5436276 0.6238397 0.5377129 0.6295848 0.5377129 0.6295848 0.5436276</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_4">0.6295848 0.5436276 0.6238397 0.5436276 0.6238397 0.5377129 0.6295848 0.5377129 0.6295848 0.5436276</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2404_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_1">0.609075 0.5546555 0.609075 0.5711168 0.609075 0.571799 0.6038104 0.571799 0.6038104 0.5546555 0.609075 0.5546555</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_1">0.609075 0.5546555 0.609075 0.5711168 0.609075 0.571799 0.6038104 0.571799 0.6038104 0.5546555 0.609075 0.5546555</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2411_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_2">0.6383208 0.5243309 0.6383208 0.5139257 0.6439665 0.5139257 0.6439665 0.5243309 0.6383208 0.5243309</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_2">0.6383208 0.5243309 0.6383208 0.5139257 0.6439665 0.5139257 0.6439665 0.5243309 0.6383208 0.5243309</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_1">0.0001466 0.462191 0.5708793 0.4621856 0.0014315 0.6858215 0.0001466 0.462191</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_1">0.0001466 0.462191 0.5708793 0.4621856 0.0014315 0.6858215 0.0001466 0.462191</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2406_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_0">0.59404 0.483888 0.59404 0.4621544 0.5968487 0.4624513 0.5968487 0.4841849 0.59404 0.483888</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_0">0.59404 0.483888 0.59404 0.4621544 0.5968487 0.4624513 0.5968487 0.4841849 0.59404 0.483888</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2426_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_3">0.7743441 0.6571935 0.7737122 0.6571935 0.7733962 0.6585565 0.7735842 0.6593674 0.7550558 0.6593674 0.7550558 0.6468558 0.7866474 0.6468558 0.7866474 0.6593674 0.7744721 0.6593674 0.7746601 0.6585565 0.7743441 0.6571935</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_3">0.7743441 0.6571935 0.7737122 0.6571935 0.7733962 0.6585565 0.7735842 0.6593674 0.7550558 0.6593674 0.7550558 0.6468558 0.7866474 0.6468558 0.7866474 0.6593674 0.7744721 0.6593674 0.7746601 0.6585565 0.7743441 0.6571935</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_4">0.6863703 0.629518 0.6863703 0.6213683 0.696884 0.6213683 0.696884 0.6231614 0.6946054 0.6231614 0.6946054 0.6234387 0.6883416 0.6234387 0.6883416 0.626606 0.6936409 0.626606 0.6936409 0.629518 0.6863703 0.629518</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_4">0.6863703 0.629518 0.6863703 0.6213683 0.696884 0.6213683 0.696884 0.6231614 0.6946054 0.6231614 0.6946054 0.6234387 0.6883416 0.6234387 0.6883416 0.626606 0.6936409 0.626606 0.6936409 0.629518 0.6863703 0.629518</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2459_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_2">0.6038104 0.4856948 0.6084512 0.4856948 0.6084512 0.4988987 0.6038125 0.4988987 0.6038104 0.4856948</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_2">0.6038104 0.4856948 0.6084512 0.4856948 0.6084512 0.4988987 0.6038125 0.4988987 0.6038104 0.4856948</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_7">0.5839 0.5203958 0.5846626 0.5203859 0.5846626 0.5607697 0.5839 0.5608097 0.5839 0.5203958</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_7">0.5839 0.5203958 0.5846626 0.5203859 0.5846626 0.5607697 0.5839 0.5608097 0.5839 0.5203958</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2408_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_3">0.6029397 0.6045877 0.5969712 0.6045877 0.5969712 0.5921208 0.6029397 0.5921208 0.6029397 0.6038894 0.6029397 0.6045877</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_3">0.6029397 0.6045877 0.5969712 0.6045877 0.5969712 0.5921208 0.6029397 0.5921208 0.6029397 0.6038894 0.6029397 0.6045877</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_4">0.8178625 0.671895 0.8178625 0.8524963 0.8031265 0.8530359 0.8033636 0.811774 0.8120757 0.81115 0.8121158 0.8041555 0.8034037 0.8047795 0.8042142 0.6636828 0.8120167 0.6631241 0.8120551 0.6561335 0.8178625 0.6557177 0.8178625 0.671895</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_4">0.8178625 0.671895 0.8178625 0.8524963 0.8031265 0.8530359 0.8033636 0.811774 0.8120757 0.81115 0.8121158 0.8041555 0.8034037 0.8047795 0.8042142 0.6636828 0.8120167 0.6631241 0.8120551 0.6561335 0.8178625 0.6557177 0.8178625 0.671895</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2402_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_0">0.6697606 0.5509989 0.6697606 0.5268441 0.6753696 0.526764 0.6753677 0.550916 0.6697606 0.5509989</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_0">0.6697606 0.5509989 0.6697606 0.5268441 0.6753696 0.526764 0.6753677 0.550916 0.6697606 0.5509989</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2415_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_0">0.9965804 0.9626331 0.9965804 0.9369732 0.9999836 0.9378403 0.9999861 0.9635037 0.9965804 0.9626331</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_0">0.9965804 0.9626331 0.9965804 0.9369732 0.9999836 0.9378403 0.9999861 0.9635037 0.9965804 0.9626331</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2465_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_4">0.6531996 0.4883507 0.6564715 0.4883507 0.6564715 0.4977375 0.6532013 0.4977375 0.6531996 0.4883507</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_4">0.6531996 0.4883507 0.6564715 0.4883507 0.6564715 0.4977375 0.6532013 0.4977375 0.6531996 0.4883507</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2412_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_1">0.6460214 0.5770469 0.6460214 0.5856098 0.6460214 0.5857042 0.641915 0.5857042 0.641915 0.5770469 0.6460214 0.5770469</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_1">0.6460214 0.5770469 0.6460214 0.5856098 0.6460214 0.5857042 0.641915 0.5857042 0.641915 0.5770469 0.6460214 0.5770469</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_10">0.7927093 0.9333442 0.8019927 0.9333425 0.8019837 0.9329816 0.8218319 0.932978 0.8311154 0.9329762 0.881298 0.9329671 0.8813016 0.9331124 0.8905802 0.9331196 0.8905764 0.9329653 0.9399485 0.9329563 0.9399819 0.9343066 0.8696752 0.9343066 0.7874939 0.9335508 0.7874941 0.9333451 0.7927093 0.9333442</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_10">0.7927093 0.9333442 0.8019927 0.9333425 0.8019837 0.9329816 0.8218319 0.932978 0.8311154 0.9329762 0.881298 0.9329671 0.8813016 0.9331124 0.8905802 0.9331196 0.8905764 0.9329653 0.9399485 0.9329563 0.9399819 0.9343066 0.8696752 0.9343066 0.7874939 0.9335508 0.7874941 0.9333451 0.7927093 0.9333442</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2466_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_0">0.6863877 0.5082203 0.6923345 0.5082203 0.6923187 0.5194612 0.6863703 0.5194647 0.6863877 0.5082203</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_0">0.6863877 0.5082203 0.6923345 0.5082203 0.6923187 0.5194612 0.6863703 0.5194647 0.6863877 0.5082203</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2409_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_2">0.5921499 0.5478172 0.5862237 0.5478172 0.5862237 0.5276511 0.5921499 0.5276511 0.5921499 0.5478172</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_2">0.5921499 0.5478172 0.5862237 0.5478172 0.5862237 0.5276511 0.5921499 0.5276511 0.5921499 0.5478172</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2402_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_3">0.6038104 0.5940338 0.6089175 0.5940338 0.6089175 0.6054977 0.6038104 0.6054977 0.6038104 0.5940338</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_3">0.6038104 0.5940338 0.6089175 0.5940338 0.6089175 0.6054977 0.6038104 0.6054977 0.6038104 0.5940338</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2467_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_2">0.5969712 0.4710525 0.6015657 0.4710525 0.6015657 0.4841909 0.5969733 0.4841909 0.5969712 0.4710525</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_2">0.5969712 0.4710525 0.6015657 0.4710525 0.6015657 0.4841909 0.5969733 0.4841909 0.5969712 0.4710525</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_13">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_13">0.6101612 0.946175 0.6297476 0.946175 0.6328824 0.946175 0.8356229 0.946175 0.8356314 0.9476885 0.6101817 0.9476886 0.6101612 0.946175</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_13">0.6101612 0.946175 0.6297476 0.946175 0.6328824 0.946175 0.8356229 0.946175 0.8356314 0.9476885 0.6101817 0.9476886 0.6101612 0.946175</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_7">0.894995 0.9555994 0.8934235 0.9555994 0.8934235 0.9501216 0.894995 0.9501216 0.894995 0.9555994</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_7">0.894995 0.9555994 0.8934235 0.9555994 0.8934235 0.9501216 0.894995 0.9501216 0.894995 0.9555994</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_17">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_17">0 0.9369777 0 0.9306569 0.0198555 0.9306569 0.0283361 0.9306569 0.0283361 0.9369777 0 0.9369777</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_17">0 0.9369777 0 0.9306569 0.0198555 0.9306569 0.0283361 0.9306569 0.0283361 0.9369777 0 0.9369777</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_31">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_31">0.6088225 0.9403893 0.6088225 0.9989182 0.6081057 0.9989182 0.6081057 0.9403893 0.6088225 0.9403893</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_31">0.6088225 0.9403893 0.6088225 0.9989182 0.6081057 0.9989182 0.6081057 0.9403893 0.6088225 0.9403893</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2459_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_0">0.6761262 0.4753776 0.6820726 0.4753739 0.682056 0.486618 0.6761114 0.486618 0.6761262 0.4753776</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2459_0">0.6761262 0.4753776 0.6820726 0.4753739 0.682056 0.486618 0.6761114 0.486618 0.6761262 0.4753776</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2415_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_3">0.8182706 0.6514274 0.8227992 0.6514274 0.8227992 0.6714677 0.8182706 0.6714677 0.8182706 0.6514274</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_3">0.8182706 0.6514274 0.8227992 0.6514274 0.8227992 0.6714677 0.8182706 0.6714677 0.8182706 0.6514274</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2425_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_3">0.8293648 0.7356263 0.8293648 0.745939 0.8246214 0.745939 0.8246214 0.7356263 0.8293648 0.7356263</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_3">0.8293648 0.7356263 0.8293648 0.745939 0.8246214 0.745939 0.8246214 0.7356263 0.8293648 0.7356263</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_0">0.5532661 0.2640211 0.0004885 0.2676399 0.0005327 0.2137781 0.5533155 0.2137781 0.5532661 0.2640211</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_0">0.5532661 0.2640211 0.0004885 0.2676399 0.0005327 0.2137781 0.5533155 0.2137781 0.5532661 0.2640211</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_5">0.8120635 0.8704331 0.8143629 0.8705862 0.8144205 0.8807786 0.5745262 0.8807786 0.5744992 0.8561538 0.6052347 0.8571408 0.6052412 0.8579698 0.6084273 0.8580644 0.6084209 0.8572432 0.664601 0.8590473 0.6674098 0.8591375 0.8021818 0.8634654 0.8022395 0.8728609 0.8103313 0.8724725 0.8120635 0.8704331</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_5">0.8120635 0.8704331 0.8143629 0.8705862 0.8144205 0.8807786 0.5745262 0.8807786 0.5744992 0.8561538 0.6052347 0.8571408 0.6052412 0.8579698 0.6084273 0.8580644 0.6084209 0.8572432 0.664601 0.8590473 0.6674098 0.8591375 0.8021818 0.8634654 0.8022395 0.8728609 0.8103313 0.8724725 0.8120635 0.8704331</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_36">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_36">0.9026956 0.9394053 0.9023983 0.9406869 0.9021012 0.9394053 0.8817782 0.9394042 0.8817782 0.9393359 0.9247892 0.9393359 0.9247892 0.9452555 0.9025278 0.9452555 0.9029927 0.9432502 0.9026956 0.9419686 0.9032899 0.9419686 0.903587 0.9406869 0.9032899 0.9394053 0.9026956 0.9394053</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_36">0.9026956 0.9394053 0.9023983 0.9406869 0.9021012 0.9394053 0.8817782 0.9394042 0.8817782 0.9393359 0.9247892 0.9393359 0.9247892 0.9452555 0.9025278 0.9452555 0.9029927 0.9432502 0.9026956 0.9419686 0.9032899 0.9419686 0.903587 0.9406869 0.9032899 0.9394053 0.9026956 0.9394053</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_4">0.8728879 0.9555994 0.8713909 0.9555994 0.8713909 0.9501216 0.8728879 0.9501216 0.8728879 0.9555994</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_4">0.8728879 0.9555994 0.8713909 0.9555994 0.8713909 0.9501216 0.8728879 0.9501216 0.8728879 0.9555994</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2465_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_1">0.6532013 0.4647202 0.6564715 0.4647202 0.6564715 0.4718435 0.653151 0.4718435 0.6532466 0.4668617 0.6532013 0.4647202</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2465_1">0.6532013 0.4647202 0.6564715 0.4647202 0.6564715 0.4718435 0.653151 0.4718435 0.6532466 0.4668617 0.6532013 0.4647202</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_11">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_11">0.7578214 0.8840919 0.7577941 0.8829997 0.8077478 0.8829997 0.8077602 0.8840001 0.8170417 0.8840017 0.8327418 0.8840047 0.8327855 0.9220897 0.8146189 0.9198872 0.8140664 0.8943806 0.8106654 0.893971 0.8112152 0.9194746 0.6954195 0.9054362 0.6953427 0.896969 0.6922083 0.8968812 0.6924716 0.9259104 0.6363611 0.92434 0.6361253 0.8983276 0.6329908 0.8982399 0.633254 0.9272692 0.6300684 0.9271876 0.6298075 0.8984116 0.6266688 0.8983299 0.626936 0.9273456 0.6073509 0.9267975 0.6071038 0.8992928 0.6039671 0.899202 0.6042299 0.9282238 0.5747419 0.9273986 0.5744992 0.8829997 0.579704 0.8829997 0.5799242 0.8919108 0.5892096 0.8919092 0.5889866 0.8829997 0.5928941 0.8829997 0.5929589 0.8989882 0.578429 0.8993568 0.5784618 0.9076357 0.5966131 0.90718 0.5966297 0.8913925 0.7162223 0.8925214 0.7160209 0.8843307 0.6782255 0.8839762 0.7485335 0.8839892 0.7485361 0.8840936 0.7578214 0.8840919</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_11">0.7578214 0.8840919 0.7577941 0.8829997 0.8077478 0.8829997 0.8077602 0.8840001 0.8170417 0.8840017 0.8327418 0.8840047 0.8327855 0.9220897 0.8146189 0.9198872 0.8140664 0.8943806 0.8106654 0.893971 0.8112152 0.9194746 0.6954195 0.9054362 0.6953427 0.896969 0.6922083 0.8968812 0.6924716 0.9259104 0.6363611 0.92434 0.6361253 0.8983276 0.6329908 0.8982399 0.633254 0.9272692 0.6300684 0.9271876 0.6298075 0.8984116 0.6266688 0.8983299 0.626936 0.9273456 0.6073509 0.9267975 0.6071038 0.8992928 0.6039671 0.899202 0.6042299 0.9282238 0.5747419 0.9273986 0.5744992 0.8829997 0.579704 0.8829997 0.5799242 0.8919108 0.5892096 0.8919092 0.5889866 0.8829997 0.5928941 0.8829997 0.5929589 0.8989882 0.578429 0.8993568 0.5784618 0.9076357 0.5966131 0.90718 0.5966297 0.8913925 0.7162223 0.8925214 0.7160209 0.8843307 0.6782255 0.8839762 0.7485335 0.8839892 0.7485361 0.8840936 0.7578214 0.8840919</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_26">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_26">0.6081057 0.9403893 0.6081057 0.9989182 0.5708335 0.9989182 0.5708335 0.9403893 0.6081057 0.9403893</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_26">0.6081057 0.9403893 0.6081057 0.9989182 0.5708335 0.9989182 0.5708335 0.9403893 0.6081057 0.9403893</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2427_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_1">0.6749511 0.6032715 0.6749511 0.6166476 0.6697606 0.6166476 0.6697606 0.6032715 0.6749511 0.6032715</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_1">0.6749511 0.6032715 0.6749511 0.6166476 0.6697606 0.6166476 0.6697606 0.6032715 0.6749511 0.6032715</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2406_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_2">0.6091841 0.4977441 0.6149768 0.4977441 0.6149768 0.5037151 0.6091841 0.5037151 0.6091841 0.4977441</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_2">0.6091841 0.4977441 0.6149768 0.4977441 0.6149768 0.5037151 0.6091841 0.5037151 0.6091841 0.4977441</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2375_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_1">0.9411879 0.9939173 0.9411879 0.9990565 0.9299388 0.9990565 0.9299388 0.9939173 0.9411879 0.9939173</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2375_1">0.9411879 0.9939173 0.9411879 0.9990565 0.9299388 0.9990565 0.9299388 0.9939173 0.9411879 0.9939173</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2457_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_2">0.6546165 0.5568153 0.6590113 0.5568153 0.6590113 0.5691509 0.6546186 0.5691509 0.6546165 0.5568153</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_2">0.6546165 0.5568153 0.6590113 0.5568153 0.6590113 0.5691509 0.6546186 0.5691509 0.6546165 0.5568153</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_4">0.9960387 0.9974576 0.992838 0.9975669 0.9927499 0.9820754 0.9894179 0.9821892 0.9894919 0.9952006 0.9843708 0.9953114 0.9843053 0.9790019 0.9473562 0.9797978 0.9474198 0.9961112 0.9425321 0.996217 0.9423547 0.9649692 0.9958532 0.9649692 0.9960387 0.9974576</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_4">0.9960387 0.9974576 0.992838 0.9975669 0.9927499 0.9820754 0.9894179 0.9821892 0.9894919 0.9952006 0.9843708 0.9953114 0.9843053 0.9790019 0.9473562 0.9797978 0.9474198 0.9961112 0.9425321 0.996217 0.9423547 0.9649692 0.9958532 0.9649692 0.9960387 0.9974576</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_8">0.0007328 0.0583215 0.0077553 0.0583215 0.0077553 0 0.1606474 0 0.1606474 0.0639404 0.0007328 0.0639404 0.0007328 0.0583215</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_8">0.0007328 0.0583215 0.0077553 0.0583215 0.0077553 0 0.1606474 0 0.1606474 0.0639404 0.0007328 0.0639404 0.0007328 0.0583215</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_0">0.5630216 0.457879 0.563019 0.4483898 0.5660022 0.4485152 0.5665959 0.4485402 0.5665959 0.4580292 0.5630216 0.457879</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_0">0.5630216 0.457879 0.563019 0.4483898 0.5660022 0.4485152 0.5665959 0.4485402 0.5665959 0.4580292 0.5630216 0.457879</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_2">0.9379611 0.9572029 0.93796 0.9564645 0.9402729 0.9564645 0.9402754 0.9572029 0.9379611 0.9572029</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_2">0.9379611 0.9572029 0.93796 0.9564645 0.9402729 0.9564645 0.9402754 0.9572029 0.9379611 0.9572029</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_10">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_10">0.7386553 0.6435523 0.7386419 0.6398662 0.7634955 0.6398662 0.7635155 0.6435523 0.7386553 0.6435523</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_10">0.7386553 0.6435523 0.7386419 0.6398662 0.7634955 0.6398662 0.7635155 0.6435523 0.7386553 0.6435523</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2467_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_4">0.5970618 0.4941612 0.6015657 0.4941612 0.6015657 0.5072995 0.5970639 0.5072995 0.5970618 0.4941612</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2467_4">0.5970618 0.4941612 0.6015657 0.4941612 0.6015657 0.5072995 0.5970639 0.5072995 0.5970618 0.4941612</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2412_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_4">0.641915 0.540146 0.6460214 0.540146 0.6460214 0.5542634 0.641915 0.5542634 0.641915 0.540146</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_4">0.641915 0.540146 0.6460214 0.540146 0.6460214 0.5542634 0.641915 0.5542634 0.641915 0.540146</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2454_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_0">0.6697726 0.4620332 0.6757011 0.4620332 0.6756846 0.4732288 0.6697606 0.473236 0.6697726 0.4620332</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_0">0.6697726 0.4620332 0.6757011 0.4620332 0.6756846 0.4732288 0.6697606 0.473236 0.6697726 0.4620332</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_5">0.7508379 0.7110705 0.6665676 0.7224523 0.5747435 0.7110705 0.7508379 0.7110705</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_5">0.7508379 0.7110705 0.6665676 0.7224523 0.5747435 0.7110705 0.7508379 0.7110705</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2426_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_5">0.7494548 0.6593674 0.7308981 0.6593674 0.7310909 0.6585358 0.730775 0.6571729 0.730143 0.6571729 0.729827 0.6585358 0.7300197 0.6593674 0.7178616 0.6593674 0.7178616 0.6468558 0.7494548 0.6468558 0.7494548 0.6593674</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_5">0.7494548 0.6593674 0.7308981 0.6593674 0.7310909 0.6585358 0.730775 0.6571729 0.730143 0.6571729 0.729827 0.6585358 0.7300197 0.6593674 0.7178616 0.6593674 0.7178616 0.6468558 0.7494548 0.6468558 0.7494548 0.6593674</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2426_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_2">0.7178616 0.6468558 0.7178616 0.6593674 0.7122619 0.6593674 0.7122619 0.6468558 0.7178616 0.6468558</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2426_2">0.7178616 0.6468558 0.7178616 0.6593674 0.7122619 0.6593674 0.7122619 0.6468558 0.7178616 0.6468558</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2423_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_3">0.8325782 0.8309003 0.8325782 0.8803356 0.8304836 0.8803356 0.8304836 0.8309003 0.8325782 0.8309003</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_3">0.8325782 0.8309003 0.8325782 0.8803356 0.8304836 0.8803356 0.8304836 0.8309003 0.8325782 0.8309003</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2424_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_4">0.5740107 0.577029 0.5740107 0.5303904 0.5801944 0.5303904 0.5801944 0.577029 0.5740107 0.577029</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_4">0.5740107 0.577029 0.5740107 0.5303904 0.5801944 0.5303904 0.5801944 0.577029 0.5740107 0.577029</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_2">0.9456134 0.9571978 0.9423587 0.9571692 0.9423547 0.9344759 0.9893156 0.9344759 0.9926479 0.9344759 0.9958489 0.9344759 0.9958548 0.9634858 0.9940908 0.9635037 0.994088 0.9480141 0.9723709 0.9482275 0.9723735 0.9574331 0.9630856 0.9573514 0.9631068 0.9453003 0.9456314 0.9451449 0.9456134 0.9571978</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_2">0.9456134 0.9571978 0.9423587 0.9571692 0.9423547 0.9344759 0.9893156 0.9344759 0.9926479 0.9344759 0.9958489 0.9344759 0.9958548 0.9634858 0.9940908 0.9635037 0.994088 0.9480141 0.9723709 0.9482275 0.9723735 0.9574331 0.9630856 0.9573514 0.9631068 0.9453003 0.9456314 0.9451449 0.9456134 0.9571978</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_9">0.7034673 0.7065095 0.7034673 0.7116789 0.700286 0.7116789 0.700286 0.7065095 0.7034673 0.7065095</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_9">0.7034673 0.7065095 0.7034673 0.7116789 0.700286 0.7116789 0.700286 0.7065095 0.7034673 0.7065095</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2451_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_0">0.6210117 0.4770777 0.6165152 0.4770465 0.6165119 0.4622871 0.6210117 0.4623207 0.6210117 0.4770777</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_0">0.6210117 0.4770777 0.6165152 0.4770465 0.6165119 0.4622871 0.6210117 0.4623207 0.6210117 0.4770777</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2411_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_4">0.6292135 0.5243309 0.6292135 0.5139257 0.6348606 0.5139257 0.6348606 0.5243309 0.6292135 0.5243309</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2411_4">0.6292135 0.5243309 0.6292135 0.5139257 0.6348606 0.5139257 0.6348606 0.5243309 0.6292135 0.5243309</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2420_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_2">0.6382719 0.5364963 0.6382719 0.5263351 0.6438859 0.5263351 0.6438859 0.5364963 0.6382719 0.5364963</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_2">0.6382719 0.5364963 0.6382719 0.5263351 0.6438859 0.5263351 0.6438859 0.5364963 0.6382719 0.5364963</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2423_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_5">0.6243283 0.635756 0.6243283 0.5863747 0.6363411 0.5863747 0.6363411 0.635756 0.6243283 0.635756</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_5">0.6243283 0.635756 0.6243283 0.5863747 0.6363411 0.5863747 0.6363411 0.635756 0.6243283 0.635756</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2408_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_1">0.6029397 0.6374019 0.5969712 0.6374019 0.5969712 0.6249335 0.6029397 0.6249335 0.6029397 0.6256372 0.6029397 0.6374019</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2408_1">0.6029397 0.6374019 0.5969712 0.6374019 0.5969712 0.6249335 0.6029397 0.6249335 0.6029397 0.6256372 0.6029397 0.6374019</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2454_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_1">0.6492806 0.4647202 0.6525582 0.4647202 0.6525582 0.4718121 0.6492428 0.4718121 0.6493321 0.4671576 0.6492806 0.4647202</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_1">0.6492806 0.4647202 0.6525582 0.4647202 0.6525582 0.4718121 0.6492428 0.4718121 0.6493321 0.4671576 0.6492806 0.4647202</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2463_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_0">0.6863866 0.548405 0.6923129 0.5484016 0.6922946 0.5596107 0.6863703 0.5596107 0.6863866 0.548405</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_0">0.6863866 0.548405 0.6923129 0.5484016 0.6922946 0.5596107 0.6863703 0.5596107 0.6863866 0.548405</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_8">0.9339221 0.9418612 0.9321843 0.9438556 0.9262335 0.9440389 0.9262335 0.9402719 0.9362525 0.940869 0.9362541 0.9420736 0.9339221 0.9418612</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_8">0.9339221 0.9418612 0.9321843 0.9438556 0.9262335 0.9440389 0.9262335 0.9402719 0.9362525 0.940869 0.9362541 0.9420736 0.9339221 0.9418612</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_5">0.9629322 0.9309833 0.9600896 0.9309833 0.9600896 0.9257908 0.9629322 0.9257908 0.9629322 0.9309833</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_5">0.9629322 0.9309833 0.9600896 0.9309833 0.9600896 0.9257908 0.9629322 0.9257908 0.9629322 0.9309833</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2423_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_0">0.9130454 0.9476886 0.9130435 0.947328 0.9378671 0.9470615 0.9378716 0.9476886 0.9130454 0.9476886</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2423_0">0.9130454 0.9476886 0.9130435 0.947328 0.9378671 0.9470615 0.9378716 0.9476886 0.9130454 0.9476886</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_29">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_29">0.030649 0.9403893 0.2572964 0.9403893 0.2572964 0.9731935 0.0310492 0.9731935 0.0310492 0.9739908 0.2572964 0.9739908 0.2572964 0.9989182 0.030649 0.9989182 0.030649 0.9403893</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_29">0.030649 0.9403893 0.2572964 0.9403893 0.2572964 0.9731935 0.0310492 0.9731935 0.0310492 0.9739908 0.2572964 0.9739908 0.2572964 0.9989182 0.030649 0.9989182 0.030649 0.9403893</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2450_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_3">0.6599932 0.5364498 0.6600847 0.5321206 0.6599921 0.5272928 0.66431 0.5272928 0.66431 0.5364498 0.6599932 0.5364498</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_3">0.6599932 0.5364498 0.6600847 0.5321206 0.6599921 0.5272928 0.66431 0.5272928 0.66431 0.5364498 0.6599932 0.5364498</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_3">0.8569222 0.9309833 0.8514532 0.9309833 0.8514532 0.9257908 0.8569222 0.9257908 0.8569222 0.9309833</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_3">0.8569222 0.9309833 0.8514532 0.9309833 0.8514532 0.9257908 0.8569222 0.9257908 0.8569222 0.9309833</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_9">0.2859796 0.1333543 0.2886614 0.1333543 0.2886614 0.1272537 0.4867506 0.1272537 0.4867506 0.1336755 0.4877676 0.1336755 0.4877676 0.1392944 0.0007328 0.1392944 0.0007328 0.0747457 0.0011477 0.0747457 0.0011477 0.1336755 0.0028669 0.1336755 0.0028669 0.1272537 0.2859796 0.1272537 0.2859796 0.1333543</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_9">0.2859796 0.1333543 0.2886614 0.1333543 0.2886614 0.1272537 0.4867506 0.1272537 0.4867506 0.1336755 0.4877676 0.1336755 0.4877676 0.1392944 0.0007328 0.1392944 0.0007328 0.0747457 0.0011477 0.0747457 0.0011477 0.1336755 0.0028669 0.1336755 0.0028669 0.1272537 0.2859796 0.1272537 0.2859796 0.1333543</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2410_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_0">0.6697606 0.4765462 0.6754485 0.4756691 0.675445 0.4988167 0.6697606 0.4996959 0.6697606 0.4765462</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_0">0.6697606 0.4765462 0.6754485 0.4756691 0.675445 0.4988167 0.6697606 0.4996959 0.6697606 0.4765462</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2428_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_1">0.5816126 0.4887406 0.5816126 0.5607648 0.58085 0.5607472 0.58085 0.5203785 0.5813153 0.52038 0.5813037 0.5035529 0.58085 0.5035514 0.58085 0.4887298 0.5816126 0.4887406</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2428_1">0.5816126 0.4887406 0.5816126 0.5607648 0.58085 0.5607472 0.58085 0.5203785 0.5813153 0.52038 0.5813037 0.5035529 0.58085 0.5035514 0.58085 0.4887298 0.5816126 0.4887406</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2406_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_3">0.6091841 0.479207 0.6149768 0.479207 0.6149768 0.4977441 0.6091841 0.4977441 0.6091841 0.479207</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_3">0.6091841 0.479207 0.6149768 0.479207 0.6149768 0.4977441 0.6091841 0.4977441 0.6091841 0.479207</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2412_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_0">0.6857605 0.5474453 0.6764694 0.5474422 0.6761114 0.5332839 0.685409 0.5332839 0.6857605 0.5474453</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2412_0">0.6857605 0.5474453 0.6764694 0.5474422 0.6761114 0.5332839 0.685409 0.5332839 0.6857605 0.5474453</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2456_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_2">0.6546165 0.5117566 0.6589954 0.5117566 0.6589954 0.524032 0.6546184 0.524032 0.6546165 0.5117566</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_2">0.6546165 0.5117566 0.6589954 0.5117566 0.6589954 0.524032 0.6546184 0.524032 0.6546165 0.5117566</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_21">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_21">0.3343971 0.9306569 0.3390423 0.9306569 0.3390423 0.9369777 0.3112145 0.9369777 0.3112145 0.9306569 0.319106 0.9306569 0.3343971 0.9306569</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_21">0.3343971 0.9306569 0.3390423 0.9306569 0.3390423 0.9369777 0.3112145 0.9369777 0.3112145 0.9306569 0.319106 0.9306569 0.3343971 0.9306569</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2414_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_0">0.6762871 0.5316302 0.6761114 0.5174134 0.6854396 0.517421 0.6856155 0.5316302 0.6762871 0.5316302</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2414_0">0.6762871 0.5316302 0.6761114 0.5174134 0.6854396 0.517421 0.6856155 0.5316302 0.6762871 0.5316302</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2409_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_1">0.5921499 0.5601687 0.5862237 0.5601687 0.5862237 0.5478172 0.5921499 0.5478172 0.5921499 0.5601687</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_1">0.5921499 0.5601687 0.5862237 0.5601687 0.5862237 0.5478172 0.5921499 0.5478172 0.5921499 0.5601687</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_2">0.8695652 0.9647202 0.8695652 0.9583066 0.8716724 0.9583066 0.8716724 0.9647202 0.8695652 0.9647202</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_2">0.8695652 0.9647202 0.8695652 0.9583066 0.8716724 0.9583066 0.8716724 0.9647202 0.8695652 0.9647202</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2455_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_2">0.6492428 0.5094099 0.6536481 0.5094099 0.6536481 0.5218072 0.6492448 0.5218072 0.6492428 0.5094099</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_2">0.6492428 0.5094099 0.6536481 0.5094099 0.6536481 0.5218072 0.6492448 0.5218072 0.6492428 0.5094099</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_6">0.8934235 0.9501216 0.8934235 0.9555994 0.8909542 0.9555994 0.8787274 0.9555994 0.8775439 0.9555994 0.8775439 0.9501216 0.8934235 0.9501216</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_6">0.8934235 0.9501216 0.8934235 0.9555994 0.8909542 0.9555994 0.8787274 0.9555994 0.8775439 0.9555994 0.8775439 0.9501216 0.8934235 0.9501216</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2452_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_3">0.6600156 0.6244226 0.6600966 0.6205896 0.6599923 0.6151535 0.6643437 0.6151535 0.6643437 0.6244226 0.6600156 0.6244226</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_3">0.6600156 0.6244226 0.6600966 0.6205896 0.6599923 0.6151535 0.6643437 0.6151535 0.6643437 0.6244226 0.6600156 0.6244226</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_24">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_24">0.244893 0.4580174 0.0008508 0.4580292 0.0007328 0.3506958 0.0015618 0.3506904 0.0015613 0.3707758 0.0027293 0.3707736 0.0028199 0.4531121 0.2428905 0.4531006 0.2424335 0.3722486 0.2999722 0.3722584 0.3004314 0.4531122 0.5585977 0.4531093 0.5584841 0.3697695 0.5604586 0.3697659 0.5605789 0.4580263 0.2984847 0.4580292 0.2980255 0.3771751 0.2721725 0.3771706 0.244436 0.3771659 0.244893 0.4580174</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_24">0.244893 0.4580174 0.0008508 0.4580292 0.0007328 0.3506958 0.0015618 0.3506904 0.0015613 0.3707758 0.0027293 0.3707736 0.0028199 0.4531121 0.2428905 0.4531006 0.2424335 0.3722486 0.2999722 0.3722584 0.3004314 0.4531122 0.5585977 0.4531093 0.5584841 0.3697695 0.5604586 0.3697659 0.5605789 0.4580263 0.2984847 0.4580292 0.2980255 0.3771751 0.2721725 0.3771706 0.244436 0.3771659 0.244893 0.4580174</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2407_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_4">0.6096727 0.5279806 0.6155114 0.5279806 0.6155114 0.5340135 0.6096727 0.5340135 0.6096727 0.5279806</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2407_4">0.6096727 0.5279806 0.6155114 0.5279806 0.6155114 0.5340135 0.6096727 0.5340135 0.6096727 0.5279806</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2458_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_3">0.6229666 0.5222413 0.623014 0.5200057 0.6228648 0.5122332 0.6274876 0.5122332 0.6274876 0.5222413 0.6229666 0.5222413</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_3">0.6229666 0.5222413 0.623014 0.5200057 0.6228648 0.5122332 0.6274876 0.5122332 0.6274876 0.5222413 0.6229666 0.5222413</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_1">0.8357943 0.9309833 0.8329262 0.9309833 0.8329262 0.9257908 0.8357943 0.9257908 0.8357943 0.9309833</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_1">0.8357943 0.9309833 0.8329262 0.9309833 0.8329262 0.9257908 0.8357943 0.9257908 0.8357943 0.9309833</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2452_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_1">0.6600177 0.5936739 0.6643437 0.5936739 0.6643437 0.602948 0.6599903 0.602948 0.6600966 0.5974063 0.6600177 0.5936739</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_1">0.6600177 0.5936739 0.6643437 0.5936739 0.6643437 0.602948 0.6599903 0.602948 0.6600966 0.5974063 0.6600177 0.5936739</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2403_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_1">0.9298956 0.9647202 0.9298956 0.9518815 0.9342964 0.9518815 0.934553 0.9518815 0.934553 0.9647202 0.9298956 0.9647202</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_1">0.9298956 0.9647202 0.9298956 0.9518815 0.9342964 0.9518815 0.934553 0.9518815 0.934553 0.9647202 0.9298956 0.9647202</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_7">0.6676849 0.5048662 0.6646264 0.5048662 0.6646264 0.472411 0.6676849 0.472411 0.6676849 0.5048662</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_7">0.6676849 0.5048662 0.6646264 0.5048662 0.6646264 0.472411 0.6676849 0.472411 0.6676849 0.5048662</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2456_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_0">0.686382 0.5216442 0.6923091 0.5216382 0.6922931 0.5328467 0.6863703 0.5328467 0.686382 0.5216442</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2456_0">0.686382 0.5216442 0.6923091 0.5216382 0.6922931 0.5328467 0.6863703 0.5328467 0.686382 0.5216442</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2457_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_0">0.7025944 0.620471 0.7025944 0.6352293 0.6980987 0.6351899 0.6980948 0.620438 0.7025944 0.620471</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_0">0.7025944 0.620471 0.7025944 0.6352293 0.6980987 0.6351899 0.6980948 0.620438 0.7025944 0.620471</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2461_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_1">0.6493378 0.5900244 0.6539069 0.5900244 0.6539069 0.6000712 0.6492428 0.6000712 0.6493891 0.5924509 0.6493378 0.5900244</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2461_1">0.6493378 0.5900244 0.6539069 0.5900244 0.6539069 0.6000712 0.6492428 0.6000712 0.6493891 0.5924509 0.6493378 0.5900244</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2417_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_2">0.6380465 0.473236 0.6380465 0.462033 0.639918 0.462033 0.639918 0.473236 0.6380465 0.473236</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2417_2">0.6380465 0.473236 0.6380465 0.462033 0.639918 0.462033 0.639918 0.473236 0.6380465 0.473236</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2410_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_4">0.6292135 0.5121654 0.6292135 0.5017673 0.6348467 0.5017673 0.6348467 0.5121654 0.6292135 0.5121654</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_4">0.6292135 0.5121654 0.6292135 0.5017673 0.6348467 0.5017673 0.6348467 0.5121654 0.6292135 0.5121654</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2455_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_0">0.6834549 0.4888005 0.6893809 0.4887913 0.6893643 0.5 0.6834392 0.5 0.6834549 0.4888005</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_0">0.6834549 0.4888005 0.6893809 0.4887913 0.6893643 0.5 0.6834392 0.5 0.6834549 0.4888005</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2415_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_2">0.8182706 0.6714677 0.8227992 0.6714677 0.8227992 0.678121 0.8182706 0.678121 0.8182706 0.6714677</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2415_2">0.8182706 0.6714677 0.8227992 0.6714677 0.8227992 0.678121 0.8182706 0.678121 0.8182706 0.6714677</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2457_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_1">0.6546554 0.5474453 0.6590113 0.5474453 0.6590113 0.5568153 0.6546165 0.5568153 0.6547293 0.5509411 0.6546554 0.5474453</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2457_1">0.6546554 0.5474453 0.6590113 0.5474453 0.6590113 0.5568153 0.6546165 0.5568153 0.6547293 0.5509411 0.6546554 0.5474453</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_7">0.5744992 0.7886494 0.8018427 0.7886494 0.8018969 0.7978988 0.6116209 0.7982794 0.6019341 0.7982988 0.6019719 0.8122494 0.6116598 0.8122255 0.6610792 0.8121033 0.6610814 0.8128618 0.6707677 0.8128482 0.6707655 0.8120794 0.7201436 0.8119573 0.7201858 0.8136448 0.7229702 0.8136456 0.7231365 0.8281077 0.7233859 0.8498031 0.726193 0.8498996 0.8022167 0.8525136 0.8022255 0.8540146 0.6674552 0.8494496 0.6672059 0.8277545 0.6643971 0.8276594 0.6646464 0.8493545 0.6084671 0.8474516 0.6083172 0.8276361 0.6051311 0.8275359 0.6052809 0.8473437 0.5745459 0.8463026 0.5744992 0.7886494</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_7">0.5744992 0.7886494 0.8018427 0.7886494 0.8018969 0.7978988 0.6116209 0.7982794 0.6019341 0.7982988 0.6019719 0.8122494 0.6116598 0.8122255 0.6610792 0.8121033 0.6610814 0.8128618 0.6707677 0.8128482 0.6707655 0.8120794 0.7201436 0.8119573 0.7201858 0.8136448 0.7229702 0.8136456 0.7231365 0.8281077 0.7233859 0.8498031 0.726193 0.8498996 0.8022167 0.8525136 0.8022255 0.8540146 0.6674552 0.8494496 0.6672059 0.8277545 0.6643971 0.8276594 0.6646464 0.8493545 0.6084671 0.8474516 0.6083172 0.8276361 0.6051311 0.8275359 0.6052809 0.8473437 0.5745459 0.8463026 0.5744992 0.7886494</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2458_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_2">0.6228627 0.499052 0.6274876 0.499052 0.6274876 0.5122332 0.6228648 0.5122332 0.6228627 0.499052</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2458_2">0.6228627 0.499052 0.6274876 0.499052 0.6274876 0.5122332 0.6228648 0.5122332 0.6228627 0.499052</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_7">0.5935515 0.5352798 0.5960253 0.5352798 0.5960253 0.560389 0.5935515 0.560389 0.5935515 0.5352798</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_7">0.5935515 0.5352798 0.5960253 0.5352798 0.5960253 0.560389 0.5935515 0.560389 0.5935515 0.5352798</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2422_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_2">0.8338243 0.9397221 0.8338243 0.9440389 0.6101612 0.9440389 0.6101612 0.9391892 0.7149246 0.9394394 0.7149326 0.9381218 0.7248927 0.938053 0.724889 0.9389014 0.7263204 0.938879 0.7263243 0.9380431 0.7363932 0.9379734 0.736384 0.9394857 0.8338243 0.9397221</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2422_2">0.8338243 0.9397221 0.8338243 0.9440389 0.6101612 0.9440389 0.6101612 0.9391892 0.7149246 0.9394394 0.7149326 0.9381218 0.7248927 0.938053 0.724889 0.9389014 0.7263204 0.938879 0.7263243 0.9380431 0.7363932 0.9379734 0.736384 0.9394857 0.8338243 0.9397221</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2427_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_0">0.6096742 0.4622871 0.6159031 0.4622871 0.6159016 0.4705955 0.6096727 0.4705955 0.6096742 0.4622871</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2427_0">0.6096742 0.4622871 0.6159031 0.4622871 0.6159016 0.4705955 0.6096727 0.4705955 0.6096742 0.4622871</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_6">0.1629214 0.0639404 0.1629214 0 0.3228377 0 0.3228377 0.03538 0.3228377 0.0434071 0.3228377 0.0639404 0.1629214 0.0639404</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_6">0.1629214 0.0639404 0.1629214 0 0.3228377 0 0.3228377 0.03538 0.3228377 0.0434071 0.3228377 0.0639404 0.1629214 0.0639404</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_34">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_34">0.2853761 0.9926138 0.2853761 0.9989182 0.2572964 0.9989182 0.2572964 0.9739908 0.2584392 0.9739908 0.2584392 0.9797044 0.2586573 0.9797044 0.2586573 0.9954103 0.2599381 0.9954103 0.2599381 0.9821227 0.2639831 0.9821227 0.274074 0.9821227 0.2853761 0.9821227 0.2853761 0.9925402 0.2853761 0.9926138</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_34">0.2853761 0.9926138 0.2853761 0.9989182 0.2572964 0.9989182 0.2572964 0.9739908 0.2584392 0.9739908 0.2584392 0.9797044 0.2586573 0.9797044 0.2586573 0.9954103 0.2599381 0.9954103 0.2599381 0.9821227 0.2639831 0.9821227 0.274074 0.9821227 0.2853761 0.9821227 0.2853761 0.9925402 0.2853761 0.9926138</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2454_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_3">0.649279 0.4882385 0.6493321 0.4857249 0.6492444 0.4811511 0.6525582 0.4811511 0.6525582 0.4882385 0.649279 0.4882385</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2454_3">0.649279 0.4882385 0.6493321 0.4857249 0.6492444 0.4811511 0.6525582 0.4811511 0.6525582 0.4882385 0.649279 0.4882385</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2463_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_2">0.7903701 0.6593674 0.7903701 0.651085 0.7941568 0.651085 0.7941568 0.6593635 0.7903701 0.6593674</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_2">0.7903701 0.6593674 0.7903701 0.651085 0.7941568 0.651085 0.7941568 0.6593635 0.7903701 0.6593674</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2406_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_4">0.6091841 0.473236 0.6149768 0.473236 0.6149768 0.479207 0.6091841 0.479207 0.6091841 0.473236</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_4">0.6091841 0.473236 0.6149768 0.473236 0.6149768 0.479207 0.6091841 0.479207 0.6091841 0.473236</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_2">0.68784 0.6021898 0.6883907 0.6029817 0.6889415 0.6037737 0.6894922 0.6045657 0.6894922 0.6055732 0.6863703 0.6055732 0.6863703 0.6043034 0.68784 0.6021898</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_2">0.68784 0.6021898 0.6883907 0.6029817 0.6889415 0.6037737 0.6894922 0.6045657 0.6894922 0.6055732 0.6863703 0.6055732 0.6863703 0.6043034 0.68784 0.6021898</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_5">0.8775439 0.9555994 0.8728879 0.9555994 0.8728879 0.9501216 0.8775439 0.9501216 0.8775439 0.9555994</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_5">0.8775439 0.9555994 0.8728879 0.9555994 0.8728879 0.9501216 0.8775439 0.9501216 0.8775439 0.9555994</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2410_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_2">0.6383 0.5121654 0.6383 0.5017673 0.6439326 0.5017673 0.6439326 0.5121654 0.6383 0.5121654</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2410_2">0.6383 0.5121654 0.6383 0.5017673 0.6439326 0.5017673 0.6439326 0.5121654 0.6383 0.5121654</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2464_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_3">0.9380066 0.9823215 0.9380538 0.9800906 0.9379595 0.9751796 0.9412785 0.9751796 0.9412785 0.9823215 0.9380066 0.9823215</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_3">0.9380066 0.9823215 0.9380538 0.9800906 0.9379595 0.9751796 0.9412785 0.9751796 0.9412785 0.9823215 0.9380066 0.9823215</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2406_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_1">0.6149768 0.5037151 0.6149768 0.5203167 0.6149768 0.5222521 0.6091841 0.5222521 0.6091841 0.5037151 0.6149768 0.5037151</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2406_1">0.6149768 0.5037151 0.6149768 0.5203167 0.6149768 0.5222521 0.6091841 0.5222521 0.6091841 0.5037151 0.6149768 0.5037151</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_2">0.6265073 0.4610705 0.628363 0.4629864 0.6289972 0.4629868 0.6289936 0.4857863 0.6233512 0.4859756 0.6233512 0.4611511 0.6265073 0.4610705</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_2">0.6265073 0.4610705 0.628363 0.4629864 0.6289972 0.4629868 0.6289936 0.4857863 0.6233512 0.4859756 0.6233512 0.4611511 0.6265073 0.4610705</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2450_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_4">0.6599932 0.5364498 0.66431 0.5364498 0.66431 0.5485049 0.6599954 0.5485049 0.6599932 0.5364498</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2450_4">0.6599932 0.5364498 0.66431 0.5364498 0.66431 0.5485049 0.6599954 0.5485049 0.6599932 0.5364498</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_0">0.5761191 0.6945557 0.590656 0.6948116 0.5907282 0.6697159 0.5744992 0.6694291 0.5745434 0.6602006 0.5939983 0.6602006 0.5938501 0.6791564 0.7138581 0.6854396 0.7140028 0.6936415 0.5943806 0.6873713 0.5942546 0.703163 0.5760944 0.7028385 0.5761191 0.6945557</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_0">0.5761191 0.6945557 0.590656 0.6948116 0.5907282 0.6697159 0.5744992 0.6694291 0.5745434 0.6602006 0.5939983 0.6602006 0.5938501 0.6791564 0.7138581 0.6854396 0.7140028 0.6936415 0.5943806 0.6873713 0.5942546 0.703163 0.5760944 0.7028385 0.5761191 0.6945557</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2373_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_4">0.6674743 0.7293187 0.7508397 0.7414281 0.5747435 0.7414281 0.6674743 0.7293187</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2373_4">0.6674743 0.7293187 0.7508397 0.7414281 0.5747435 0.7414281 0.6674743 0.7293187</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2421_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_6">0.6218107 0.5302215 0.6160234 0.5302215 0.6160234 0.4805353 0.6218107 0.4805353 0.6218107 0.5302215</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2421_6">0.6218107 0.5302215 0.6160234 0.5302215 0.6160234 0.4805353 0.6218107 0.4805353 0.6218107 0.5302215</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2424_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_2">0.5740107 0.5903934 0.5801944 0.5903934 0.5801944 0.6370354 0.5740107 0.6370354 0.5740107 0.5903934</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2424_2">0.5740107 0.5903934 0.5801944 0.5903934 0.5801944 0.6370354 0.5740107 0.6370354 0.5740107 0.5903934</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2416_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_4">0.6546165 0.5924574 0.659127 0.5924574 0.659127 0.5971379 0.6546165 0.5971379 0.6546165 0.5924574</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_4">0.6546165 0.5924574 0.659127 0.5924574 0.659127 0.5971379 0.6546165 0.5971379 0.6546165 0.5924574</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2409_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_0">0.6764657 0.5948905 0.6761114 0.5807273 0.6854087 0.5807365 0.6857589 0.5948905 0.6764657 0.5948905</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2409_0">0.6764657 0.5948905 0.6761114 0.5807273 0.6854087 0.5807365 0.6857589 0.5948905 0.6764657 0.5948905</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2452_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_0">0.6575645 0.4608111 0.6634891 0.4608111 0.6634713 0.4720137 0.6575477 0.4720195 0.6575645 0.4608111</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2452_0">0.6575645 0.4608111 0.6634891 0.4608111 0.6634713 0.4720137 0.6575477 0.4720195 0.6575645 0.4608111</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_30">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_30">0 0.9989182 0 0.9403893 0.030649 0.9403893 0.030649 0.9989182 0 0.9989182</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_30">0 0.9989182 0 0.9403893 0.030649 0.9403893 0.030649 0.9989182 0 0.9989182</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2416_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_1">0.659127 0.619228 0.659127 0.6243061 0.659127 0.6366378 0.6546165 0.6366378 0.6546165 0.619228 0.659127 0.619228</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_1">0.659127 0.619228 0.659127 0.6243061 0.659127 0.6366378 0.6546165 0.6366378 0.6546165 0.619228 0.659127 0.619228</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2403_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_3">0.9171864 0.9647202 0.9171864 0.9518815 0.9174395 0.9518815 0.9218428 0.9518815 0.9218428 0.9647202 0.9171864 0.9647202</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_3">0.9171864 0.9647202 0.9171864 0.9518815 0.9174395 0.9518815 0.9218428 0.9518815 0.9218428 0.9647202 0.9171864 0.9647202</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2405_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_2">0.6295848 0.5678934 0.6238397 0.5678934 0.6238397 0.5619787 0.6295848 0.5619787 0.6295848 0.5678934</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2405_2">0.6295848 0.5678934 0.6238397 0.5678934 0.6238397 0.5619787 0.6295848 0.5619787 0.6295848 0.5678934</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2402_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_1">0.6038104 0.62532 0.6089175 0.62532 0.6089175 0.6367801 0.6038104 0.6367801 0.6038104 0.62532</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2402_1">0.6038104 0.62532 0.6089175 0.62532 0.6089175 0.6367801 0.6038104 0.6367801 0.6038104 0.62532</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2462_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_0">0.6863859 0.5617912 0.6923112 0.5617912 0.6922943 0.5729861 0.6863703 0.5729927 0.6863859 0.5617912</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2462_0">0.6863859 0.5617912 0.6923112 0.5617912 0.6922943 0.5729861 0.6863703 0.5729927 0.6863859 0.5617912</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2403_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_0">0.6697606 0.5765569 0.6697618 0.552406 0.6753708 0.5523114 0.6753708 0.5764678 0.6697606 0.5765569</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2403_0">0.6697606 0.5765569 0.6697618 0.552406 0.6753708 0.5523114 0.6753708 0.5764678 0.6697606 0.5765569</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2425_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_4">0.8293648 0.698398 0.8293648 0.7356263 0.8246214 0.7356263 0.8246214 0.698398 0.8293648 0.698398</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2425_4">0.8293648 0.698398 0.8293648 0.7356263 0.8246214 0.7356263 0.8246214 0.698398 0.8293648 0.698398</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2376_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_9">0.6860795 0.5969919 0.6861304 0.6362531 0.6761114 0.6356559 0.6761114 0.5967376 0.6860795 0.5969919</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2376_9">0.6860795 0.5969919 0.6861304 0.6362531 0.6761114 0.6356559 0.6761114 0.5967376 0.6860795 0.5969919</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2418_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_0">0.9997222 0.9975669 0.9965847 0.9973027 0.9965804 0.9682364 0.9997222 0.9684948 0.9997222 0.9975669</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2418_0">0.9997222 0.9975669 0.9965847 0.9973027 0.9965804 0.9682364 0.9997222 0.9684948 0.9997222 0.9975669</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2420_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_4">0.6292135 0.5364963 0.6292135 0.5263351 0.6348301 0.5263351 0.6348301 0.5364963 0.6292135 0.5364963</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2420_4">0.6292135 0.5364963 0.6292135 0.5263351 0.6348301 0.5263351 0.6348301 0.5364963 0.6292135 0.5364963</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_9">0.9083439 0.9583066 0.9083439 0.9647202 0.8808411 0.9647202 0.8808411 0.9583066 0.8842974 0.9583066 0.8848497 0.9583066 0.8856396 0.9583066 0.8991395 0.9583066 0.904313 0.9583066 0.9049413 0.9583066 0.9083439 0.9583066</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_9">0.9083439 0.9583066 0.9083439 0.9647202 0.8808411 0.9647202 0.8808411 0.9583066 0.8842974 0.9583066 0.8848497 0.9583066 0.8856396 0.9583066 0.8991395 0.9583066 0.904313 0.9583066 0.9049413 0.9583066 0.9083439 0.9583066</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2464_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_4">0.9380066 0.9823215 0.9412785 0.9823215 0.9412785 0.9917285 0.9380084 0.9917285 0.9380066 0.9823215</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2464_4">0.9380066 0.9823215 0.9412785 0.9823215 0.9412785 0.9917285 0.9380084 0.9917285 0.9380066 0.9823215</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_11">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_11">0.7260497 0.7116789 0.7205356 0.7116789 0.7173988 0.7116789 0.7173988 0.7065095 0.7260497 0.7065095 0.7260497 0.7116789</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_11">0.7260497 0.7116789 0.7205356 0.7116789 0.7173988 0.7116789 0.7173988 0.7065095 0.7260497 0.7065095 0.7260497 0.7116789</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2416_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_2">0.6546165 0.6145476 0.659127 0.6145476 0.659127 0.619228 0.6546165 0.619228 0.6546165 0.6145476</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_2">0.6546165 0.6145476 0.659127 0.6145476 0.659127 0.619228 0.6546165 0.619228 0.6546165 0.6145476</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2451_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_1">0.6600065 0.5498784 0.6643255 0.5498784 0.6643255 0.5590576 0.6599903 0.5590576 0.6600903 0.5538439 0.6600065 0.5498784</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2451_1">0.6600065 0.5498784 0.6643255 0.5498784 0.6643255 0.5590576 0.6599903 0.5590576 0.6600903 0.5538439 0.6600065 0.5498784</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2404_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_0">0.8242332 0.6399027 0.8325319 0.6399027 0.8324315 0.6478434 0.8241329 0.6478434 0.8242332 0.6399027</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2404_0">0.8242332 0.6399027 0.8325319 0.6399027 0.8324315 0.6478434 0.8241329 0.6478434 0.8242332 0.6399027</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2431_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_8">0.700286 0.7116789 0.6835856 0.7116789 0.6835856 0.7065095 0.700286 0.7065095 0.700286 0.7116789</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2431_8">0.700286 0.7116789 0.6835856 0.7116789 0.6835856 0.7065095 0.700286 0.7065095 0.700286 0.7116789</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2463_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_3">0.7970338 0.6592244 0.7962209 0.6591178 0.7941568 0.6593635 0.7941568 0.651085 0.7970338 0.651085 0.7970338 0.6592244</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2463_3">0.7970338 0.6592244 0.7962209 0.6591178 0.7941568 0.6593635 0.7941568 0.651085 0.7970338 0.651085 0.7970338 0.6592244</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2455_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_4">0.6492794 0.5312248 0.6536481 0.5312248 0.6536481 0.5436243 0.6492817 0.5436243 0.6492794 0.5312248</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2455_4">0.6492794 0.5312248 0.6536481 0.5312248 0.6536481 0.5436243 0.6492817 0.5436243 0.6492794 0.5312248</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2466_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_1">0.6366459 0.540146 0.641132 0.540146 0.641132 0.5501032 0.6365413 0.5501032 0.6366913 0.5422896 0.6366459 0.540146</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2466_1">0.6366459 0.540146 0.641132 0.540146 0.641132 0.5501032 0.6365413 0.5501032 0.6366913 0.5422896 0.6366459 0.540146</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2416_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_0">0.665364 0.5909765 0.665364 0.5618967 0.6685014 0.5621611 0.6685014 0.5912409 0.665364 0.5909765</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2416_0">0.665364 0.5909765 0.665364 0.5618967 0.6685014 0.5621611 0.6685014 0.5912409 0.665364 0.5909765</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2430_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_0">0.5813385 0.561442 0.5844179 0.5615538 0.5843428 0.5741002 0.5835457 0.578695 0.5834553 0.6292684 0.5894096 0.6291511 0.591152 0.6271749 0.5934849 0.6274134 0.5934814 0.6344391 0.5911818 0.6341983 0.5894364 0.6361727 0.5813385 0.6362531 0.5813385 0.561442</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2430_0">0.5813385 0.561442 0.5844179 0.5615538 0.5843428 0.5741002 0.5835457 0.578695 0.5834553 0.6292684 0.5894096 0.6291511 0.591152 0.6271749 0.5934849 0.6274134 0.5934814 0.6344391 0.5911818 0.6341983 0.5894364 0.6361727 0.5813385 0.6362531 0.5813385 0.561442</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0275_p2429_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_3">0.6889415 0.6353695 0.6894922 0.6345775 0.6894921 0.6329936 0.6889414 0.6322016 0.6883907 0.6329936 0.6883907 0.6345775 0.68784 0.6337855 0.6863703 0.6358991 0.6863703 0.629518 0.6936409 0.629518 0.6936409 0.637169 0.6894922 0.637169 0.6894922 0.6361614 0.6889415 0.6353695</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0275_p2429_3">0.6889415 0.6353695 0.6894922 0.6345775 0.6894921 0.6329936 0.6889414 0.6322016 0.6883907 0.6329936 0.6883907 0.6345775 0.68784 0.6337855 0.6863703 0.6358991 0.6863703 0.629518 0.6936409 0.629518 0.6936409 0.637169 0.6894922 0.637169 0.6894922 0.6361614 0.6889415 0.6353695</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
				</app:ParameterizedTexture>
			</app:surfaceDataMember>
			<app:surfaceDataMember>
				<app:ParameterizedTexture>
					<app:imageURI>53392642_bldg_6697_appearance/hnap0034.tif</app:imageURI>
					<app:mimeType>image/tif</app:mimeType>
					<app:target uri="#poly_HNAP0034_p2437_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_2">0.3794872 0.2083333 0.3794872 0.0695513 0.4086272 0.0695513 0.4086272 0.2083333 0.3794872 0.2083333</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_2">0.3794872 0.2083333 0.3794872 0.0695513 0.4086272 0.0695513 0.4086272 0.2083333 0.3794872 0.2083333</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2438_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_0">0.3297569 0.9980752 0.3181555 1 0.3158179 0.0698929 0.3274193 0.0679681 0.3297569 0.9980752</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_0">0.3297569 0.9980752 0.3181555 1 0.3158179 0.0698929 0.3274193 0.0679681 0.3297569 0.9980752</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2434_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_4">0.6646057 1 0.4292591 1 0.4292591 0.6315125 0.5594041 0.6315125 0.5594041 0.7873768 0.585743 0.7873768 0.585743 0.6315125 0.6646057 0.6315125 0.6646057 1</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_4">0.6646057 1 0.4292591 1 0.4292591 0.6315125 0.5594041 0.6315125 0.5594041 0.7873768 0.585743 0.7873768 0.585743 0.6315125 0.6646057 0.6315125 0.6646057 1</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2434_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_2">0.990773 1 0.7554402 1 0.7554402 0.6315125 0.9046779 0.6315125 0.9046779 0.7873768 0.990773 0.7873768 0.990773 1</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_2">0.990773 1 0.7554402 1 0.7554402 0.6315125 0.9046779 0.6315125 0.9046779 0.7873768 0.990773 0.7873768 0.990773 1</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2438_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_2">0.5533957 0.5833333 0.5435067 0.5833333 0.5435067 0.3010436 0.5533957 0.3010436 0.5533957 0.5833333</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_2">0.5533957 0.5833333 0.5435067 0.5833333 0.5435067 0.3010436 0.5533957 0.3010436 0.5533957 0.5833333</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2434_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_1">0.0024039 0.936795 0 0.0679681 0.2770963 0.0895508 0.2795169 0.9580197 0.0024039 0.936795</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_1">0.0024039 0.936795 0 0.0679681 0.2770963 0.0895508 0.2795169 0.9580197 0.0024039 0.936795</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2438_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_4">0.4459286 0.5833333 0.4360396 0.5833333 0.4360396 0.3010436 0.4459286 0.3010436 0.4459286 0.5833333</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_4">0.4459286 0.5833333 0.4360396 0.5833333 0.4360396 0.3010436 0.4459286 0.3010436 0.4459286 0.5833333</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2437_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_3">0.0952621 0.9439072 0.1262753 0.9462826 0.0952679 0.9461853 0.0952621 0.9439072</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_3">0.0952621 0.9439072 0.1262753 0.9462826 0.0952679 0.9461853 0.0952621 0.9439072</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2434_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_3">0.7554402 1 0.6646057 1 0.6646057 0.6315125 0.7554402 0.6315125 0.7554402 1</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_3">0.7554402 1 0.6646057 1 0.6646057 0.6315125 0.7554402 0.6315125 0.7554402 1</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2438_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_1">0.4360396 0.3010436 0.4360396 0.5833333 0.3384615 0.5833333 0.3384615 0.3010436 0.4360396 0.3010436</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_1">0.4360396 0.3010436 0.4360396 0.5833333 0.3384615 0.5833333 0.3384615 0.3010436 0.4360396 0.3010436</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2438_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_3">0.451501 0.4574546 0.5422231 0.4574546 0.5422231 0.3010436 0.5435067 0.3010436 0.5435067 0.5833333 0.4459286 0.5833333 0.4459286 0.3010436 0.451501 0.3010436 0.451501 0.4574546</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2438_3">0.451501 0.4574546 0.5422231 0.4574546 0.5422231 0.3010436 0.5435067 0.3010436 0.5435067 0.5833333 0.4459286 0.5833333 0.4459286 0.3010436 0.451501 0.3010436 0.451501 0.4574546</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2437_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_1">0.5641026 0.0416666 0.573792 0.0416666 0.573792 0.5300375 0.5641026 0.5300375 0.5641026 0.0416666</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_1">0.5641026 0.0416666 0.573792 0.0416666 0.573792 0.5300375 0.5641026 0.5300375 0.5641026 0.0416666</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2434_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_0">0.3384615 0.7873768 0.4280822 0.7873768 0.4280822 0.6315125 0.4292591 0.6315125 0.4292591 1 0.3384615 1 0.3384615 0.7873768</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2434_0">0.3384615 0.7873768 0.4280822 0.7873768 0.4280822 0.6315125 0.4292591 0.6315125 0.4292591 1 0.3384615 1 0.3384615 0.7873768</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2437_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_4">0.3650133 0.0942484 0.3650133 0.25 0.3647753 0.25 0.3647753 0.0942484 0.3650133 0.0942484</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_4">0.3650133 0.0942484 0.3650133 0.25 0.3647753 0.25 0.3647753 0.0942484 0.3650133 0.0942484</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2437_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_5">0.3384615 0.25 0.3384615 0.0942484 0.3647753 0.0942484 0.3647753 0.25 0.3384615 0.25</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_5">0.3384615 0.25 0.3384615 0.0942484 0.3647753 0.0942484 0.3647753 0.25 0.3384615 0.25</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0034_p2437_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_0">0.3158487 0.0821279 0.318022 0.9468841 0.2794855 0.9467632 0.2770963 0.0895508 0.1757223 0.0816549 0.3158487 0.0821279</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0034_p2437_0">0.3158487 0.0821279 0.318022 0.9468841 0.2794855 0.9467632 0.2770963 0.0895508 0.1757223 0.0816549 0.3158487 0.0821279</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
				</app:ParameterizedTexture>
			</app:surfaceDataMember>
			<app:surfaceDataMember>
				<app:ParameterizedTexture>
					<app:imageURI>53392642_bldg_6697_appearance/hnap0286.tif</app:imageURI>
					<app:mimeType>image/tif</app:mimeType>
					<app:target uri="#poly_HNAP0286_p2525_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_3">0.7165818 0.8737864 0.9935137 0.8737864 0.9935137 0.9921312 0.7165818 0.9921312 0.7165818 0.8737864</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_3">0.7165818 0.8737864 0.9935137 0.8737864 0.9935137 0.9921312 0.7165818 0.9921312 0.7165818 0.8737864</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0286_p2525_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_6">0.4431837 0.1165048 0.3748155 0.1165048 0.3748155 0.0183564 0.4431837 0.0183564 0.4431837 0.1165048</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_6">0.4431837 0.1165048 0.3748155 0.1165048 0.3748155 0.0183564 0.4431837 0.0183564 0.4431837 0.1165048</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0286_p2525_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_4">0.825341 0.1165048 0.6617427 0.1165048 0.6617427 0.0183564 0.825341 0.0183564 0.825341 0.1165048</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_4">0.825341 0.1165048 0.6617427 0.1165048 0.6617427 0.0183564 0.825341 0.0183564 0.825341 0.1165048</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0286_p2525_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_7">0.3748155 0.1165048 0 0.1165048 0 0.0183564 0.3748155 0.0183564 0.3748155 0.1165048</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_7">0.3748155 0.1165048 0 0.1165048 0 0.0183564 0.3748155 0.0183564 0.3748155 0.1165048</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0286_p2525_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_5">0.6617427 0.1165048 0.4431837 0.1165048 0.4431837 0.0183564 0.6617427 0.0183564 0.6617427 0.1165048</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_5">0.6617427 0.1165048 0.4431837 0.1165048 0.4431837 0.0183564 0.6617427 0.0183564 0.6617427 0.1165048</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0286_p2525_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_0">0.9593215 0.8355901 0.3571128 0.8446602 0.3562416 0.634567 0.0050761 0.6346357 0.0050761 0.1318874 0.3740837 0.1261181 0.3750204 0.3514569 0.9594421 0.3524168 0.9593215 0.8355901</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_0">0.9593215 0.8355901 0.3571128 0.8446602 0.3562416 0.634567 0.0050761 0.6346357 0.0050761 0.1318874 0.3740837 0.1261181 0.3750204 0.3514569 0.9594421 0.3524168 0.9593215 0.8355901</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0286_p2525_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_8">0 0.8737864 0.189582 0.8737864 0.189582 0.9921312 0 0.9921312 0 0.8737864</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_8">0 0.8737864 0.189582 0.8737864 0.189582 0.9921312 0 0.9921312 0 0.8737864</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0286_p2525_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_1">0.189582 0.8737864 0.6281631 0.8737864 0.6281631 0.9921312 0.189582 0.9921312 0.189582 0.8737864</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_1">0.189582 0.8737864 0.6281631 0.8737864 0.6281631 0.9921312 0.189582 0.9921312 0.189582 0.8737864</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0286_p2525_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_2">0.6281631 0.8737864 0.7165818 0.8737864 0.7165818 0.9921312 0.6281631 0.9921312 0.6281631 0.8737864</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0286_p2525_2">0.6281631 0.8737864 0.7165818 0.8737864 0.7165818 0.9921312 0.6281631 0.9921312 0.6281631 0.8737864</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
				</app:ParameterizedTexture>
			</app:surfaceDataMember>
			<app:surfaceDataMember>
				<app:ParameterizedTexture>
					<app:imageURI>53392642_bldg_6697_appearance/hnap0279.tif</app:imageURI>
					<app:mimeType>image/tif</app:mimeType>
					<app:target uri="#poly_HNAP0279_p2471_0">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_0">0.7863528 0.7949308 0.0020945 0.9887004 0 0.7948718 0.7863528 0.7949308</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_0">0.7863528 0.7949308 0.0020945 0.9887004 0 0.7948718 0.7863528 0.7949308</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_4">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_4">0.9042678 0.9488099 0.9772764 0.9807692 0.8140495 0.9807692 0.9042678 0.9488099</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_4">0.9042678 0.9488099 0.9772764 0.9807692 0.8140495 0.9807692 0.9042678 0.9488099</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_7">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_7">0.4366279 0.4490993 0.0123967 0.4490993 0.0123967 0.3397436 0.4366279 0.3397436 0.4366279 0.4490993</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_7">0.4366279 0.4490993 0.0123967 0.4490993 0.0123967 0.3397436 0.4366279 0.3397436 0.4366279 0.4490993</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_5">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_5">0.9041962 0.8782051 0.8140495 0.8461862 0.9772291 0.8461862 0.9041962 0.8782051</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_5">0.9041962 0.8782051 0.8140495 0.8461862 0.9772291 0.8461862 0.9041962 0.8782051</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_2">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_2">0.0019965 0.5257065 0.7714618 0.525641 0 0.7574353 0.0019965 0.5257065</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_2">0.0019965 0.5257065 0.7714618 0.525641 0 0.7574353 0.0019965 0.5257065</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_3">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_3">0 0.7574353 0.7714618 0.525641 0.7714618 0.7575439 0 0.7574353</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_3">0 0.7574353 0.7714618 0.525641 0.7714618 0.7575439 0 0.7574353</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_8">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_8">0.3391703 0.1474359 0.1942149 0.1474359 0.1942149 0.0380802 0.3391703 0.0380802 0.3391703 0.1474359</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_8">0.3391703 0.1474359 0.1942149 0.1474359 0.1942149 0.0380802 0.3391703 0.0380802 0.3391703 0.1474359</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_1">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_1">0.7864082 0.9887004 0.0020945 0.9887004 0.7863528 0.7949308 0.7864082 0.9887004</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_1">0.7864082 0.9887004 0.0020945 0.9887004 0.7863528 0.7949308 0.7864082 0.9887004</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_6">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_6">0.0123967 0.0380802 0.1573942 0.0380802 0.1573942 0.1474359 0.0123967 0.1474359 0.0123967 0.0380802</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_6">0.0123967 0.0380802 0.1573942 0.0380802 0.1573942 0.1474359 0.0123967 0.1474359 0.0123967 0.0380802</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
					<app:target uri="#poly_HNAP0279_p2471_9">
						<app:TexCoordList>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_9">0.0123967 0.1602564 0.4366257 0.1602564 0.4366257 0.2696121 0.0123967 0.2696121 0.0123967 0.1602564</app:textureCoordinates>
							<app:textureCoordinates ring="#line_HNAP0279_p2471_9">0.0123967 0.1602564 0.4366257 0.1602564 0.4366257 0.2696121 0.0123967 0.2696121 0.0123967 0.1602564</app:textureCoordinates>
						</app:TexCoordList>
					</app:target>
				</app:ParameterizedTexture>
			</app:surfaceDataMember>
		</app:Appearance>
	</app:appearanceMember>
</core:CityModel>
