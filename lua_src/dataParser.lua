function main()
  
  local openPath 	 = "C:/Projects/Lua/dataOpen.txt"
  local closePath  = "C:/Projects/Lua/dataClose.txt"
  local volumePath = "C:/Projects/Lua/dataVolume.txt"
  local highPath 	 = "C:/Projects/Lua/dataHigh.txt"
  local lowPath 	 = "C:/Projects/Lua/dataLow.txt"
  
  local openIO 	 = io.open(openPath,   "w")
  local highIO 	 = io.open(highPath,   "w")
  local lowIO 	 = io.open(lowPath,    "w")
  local closeIO  = io.open(closePath,  "w")
  local volumeIO = io.open(volumePath, "w")

  ds, errorDesk = CreateDataSource("QJSIM", "SBER", INTERVAL_M1)
  local tag = "sberprice"
  if ds == nil then
    message('[Connection error]: ' .. errorDesk)
  end

  while (errorDesk == "" or errorDesk == nil) and ds:Size() == 0 do
    sleep(1)
  end
  if ((errorDesk ~= "") and (errorDesk ~= nil)) then 
    message ('[Unable to connect to the chart...]: ' .. errorDesk)
    return 0
  end

  local try_count = 0
  while ((ds:Size() == 0) and (try_count < 1000)) do
    sleep(100)
    try_count = try_count + 1
  end

  local firstCandleIndex = nil
  local maxCandles = math.min(1000, ds:Size())
  local tLines = getLinesCount(tag)
  local candlesTotal = getNumCandles(tag)
  local coveredCandles = 70

  tableCandle, n, lgnd = getCandlesByIndex(tag, 0, 0, candlesTotal)

  for i = n - coveredCandles, n - 1 do
  	local dateCandle = tostring(tableCandle[i].datetime.hour)..":"..tostring(tableCandle[i].datetime.min)

  	openIO:write(tableCandle[i].open.."\n")-- 	 .."\t["..i.."]\t["..dateCandle.."]\n")
  	highIO:write(tableCandle[i].high.."\n")-- 	 .."\t["..i.."]\t["..dateCandle.."]\n")
  	lowIO:write(tableCandle[i].low.."\n")-- 		 .."\t["..i.."]\t["..dateCandle.."]\n")
  	closeIO:write(tableCandle[i].close.."\n")-- 	 .."\t["..i.."]\t["..dateCandle.."]\n")
  	volumeIO:write(tableCandle[i].volume.."\n")-- .."\t["..i.."]\t["..dateCandle.."]\n")
  end

  --f:write(data)

  openIO:close()
  highIO:close()
  lowIO:close()
  closeIO:close()
  volumeIO:close()
end