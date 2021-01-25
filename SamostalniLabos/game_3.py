from sys import argv

import requests
import random
from collections import defaultdict


def get(url):
    r = requests.get(url)
    res = r.json()
    print(res)
    return res


class Tile:
    def __init__(self, restype, weight, x, y):
        self.type = restype
        self.weight = weight
        self.x = x
        self.y = y


class Intersection:
    def __init__(self, index, neighbors):
        self.tiles = []
        self.neighbors = neighbors
        self.index = index
        self.city = None
        self.level = 1
        self.roads = set()


class Map:
    def __init__(self, data):
        self.wheat = 0
        self.sheep = 0
        self.wood = 0
        self.clay = 0
        self.iron = 0
        self.wheatGen = 0
        self.sheepGen = 0
        self.woodGen = 0
        self.clayGen = 0
        self.ironGen = 0

        def getType(name):
            if name == 'WATER':
                return 0
            elif name == "DUST":
                return 1
            elif name == "SHEEP":
                return 2
            elif name == "WOOD":
                return 3
            elif name == "WHEAT":
                return 4
            elif name == "CLAY":
                return 5
            return 6

        self.miBuilder = None
        self.viBuilder = None

        self.intersections = []
        for i, relation in enumerate(data['indexMap']):
            self.intersections.append(Intersection(i, relation))

        xtiles = defaultdict(lambda: defaultdict(Tile))
        for i in range(len(data['map']['tiles'])):
            for relation in data['map']['tiles'][i]:
                if not relation:    continue
                if relation['x'] in xtiles and relation['y'] in xtiles[relation['x']]:  continue
                xtiles[relation['x']][relation['y']] = Tile(getType(relation['resourceType']),
                                                            relation['resourceWeight'], relation['x'], relation['y'])

        for i, tiles in enumerate(data['intersectionCoordinates']):
            for tile in tiles:
                self.intersections[i].tiles.append(xtiles[tile['x']][tile['y']])

    def getIntersection(self, index):
        return self.intersections[index]

    def getMyBestTown(self):
        candidates = []
        for intersection in self.intersections:
            if intersection.city is True and intersection.level == 1:
                candidates.append(intersection)
        if not candidates:
            return None

        # sparestResource = min(self.wood, self.clay, self.sheep, self.wheat, self.iron)
        # maxTile = candidates[0]
        # maxWeight = 0
        # for tile in candidates[0].tiles:
        #     if tile.type == sparestResource and tile.weight > maxWeight:
        #         maxWeight = tile.weight
        #
        # for candidate in candidates[1:]:
        #     tiles = candidate.tiles
        #     for tile in tiles:
        #         if tile.type == sparestResource and tile.weight > maxWeight:
        #             maxTile = candidate
        #             maxWeight = tile.weight
        # return maxTile
        return candidates[random.randint(0, len(candidates) - 1)]

    def getNeighborIntersections(self, index):
        nxt = self.getIntersection(index)
        result = []
        for num in nxt.neighbors:
            result.append(self.getIntersection(num))
        return result

    def getLayerNeighbors(self, index, layer):
        visited = {index}
        for _ in range(layer):
            result = set()
            for n in visited:
                nxt = self.getIntersection(n)
                for num in nxt.neighbors:
                    if num in visited:  continue
                    result.add(num)
            visited = visited.union(result)
        return [self.getIntersection(x) for x in result]

    def buildRoad(self, city1, city2, us, init=False):
        if not init and us:
            self.wood -= 100
            self.clay -= 100
        self.intersections[city1].roads.add((city2, us))
        self.intersections[city2].roads.add((city1, us))

    def buildCity(self, index, us, init=False):
        inte = self.getIntersection(index)
        inte.city = us
        if us:
            if not init:
                self.sheep -= 100
                self.wood -= 100
                self.wheat -= 100
                self.clay -= 100
            tiles = inte.tiles
            for tile in tiles:
                if tile.type == 2:
                    self.sheepGen += tile.weight
                elif tile.type == 3:
                    self.woodGen += tile.weight
                elif tile.type == 4:
                    self.wheatGen += tile.weight
                elif tile.type == 5:
                    self.clayGen += tile.weight
                elif tile.type == 6:
                    self.ironGen += tile.weight

    def move(self, index):
        self.miBuilder = index

    def enemyMove(self, index):
        self.viBuilder = index

    def canBuildTown(self, index, init=False):
        inter = self.getIntersection(index)
        if inter.city is not None:  return False

        for n in self.getNeighborIntersections(index):
            if n.city is not None:  return False

        if not init and (self.clay < 100 or self.wood < 100 or self.wheat < 100 or self.sheep < 100):
            return False

        oppRoad = 0
        for road in inter.roads:
            city, us = road
            if not us:  oppRoad += 1
        if oppRoad >= 2:    return False
        return True

    def canBuildRoad(self, index, init=False):
        inte = self.getIntersection(self.miBuilder)
        if not init and (self.clay < 100 or self.wood < 100):
            return False
        if (index, True) in inte.roads or (index, False) in inte.roads: return False
        return index in inte.neighbors

    def canBuildRoad2(self, i1, i2):
        inte = self.getIntersection(i1)
        if (i2, True) in inte.roads or (i2, False) in inte.roads: return False
        return i2 in inte.neighbors

    def isRoadBlocked(self, index):
        inte = self.getIntersection(self.miBuilder)
        if (index, True) in inte.roads or (index, False) in inte.roads: return True
        return index not in inte.neighbors

    def canMove(self, index):
        inte = self.getIntersection(self.miBuilder)
        return index in inte.neighbors

    def upgradeTown(self, index):
        self.getIntersection(index).level = 2
        inte = self.getIntersection(index)
        self.wheat -= 200
        self.iron -= 300
        for tile in inte.tiles:
            if tile.type == 2:
                self.sheepGen += tile.weight
            elif tile.type == 3:
                self.woodGen += tile.weight
            elif tile.type == 4:
                self.wheatGen += tile.weight
            elif tile.type == 5:
                self.clayGen += tile.weight
            elif tile.type == 6:
                self.ironGen += tile.weight

    def canUpgradeTown(self, index):
        inte = self.getIntersection(index)
        if self.wheat < 200 or self.iron < 300:
            return False
        return inte.level == 1 and inte.city is not None and inte.city

    def generateResources(self):
        self.wheat += self.wheatGen
        self.wood += self.woodGen
        self.sheep += self.sheepGen
        self.iron += self.ironGen
        self.clay += self.clayGen

    def getValidRoades(self, idx):
        inte = self.getIntersection(idx)
        neighs = inte.neighbors
        valid = []
        for n in neighs:
            if (n, True) in inte.roads or (n, False) in inte.roads: continue
            valid.append(n)
        return valid

    def getMyRoades(self, idx):
        inte = self.getIntersection(idx)
        neighs = inte.neighbors
        valid = []
        for n in neighs:
            if (n, True) in inte.roads:
                valid.append(n)
        return valid


class AbstractClient:
    def __init__(self, url, playerId, gameId):
        self.url = url
        self.playerId = playerId
        self.gameId = gameId

    def doInitial(self, x, y):
        return get(self.getDoActionUrl(f'initial+{x}+{y}'))

    def doMove(self, x):
        return get(self.getDoActionUrl(f'move+{x}'))

    def doBuildRoad(self, x):
        return get(self.getDoActionUrl(f'buildroad+{x}'))

    def doBuildTown(self):
        return get(self.getDoActionUrl(f'buildtown'))

    def doUpgradeTown(self, x):
        return get(self.getDoActionUrl(f'upgradetown+{x}'))

    def doEmpty(self):
        return get(self.getDoActionUrl(f'empty'))


class DummyClient(AbstractClient):
    def __init__(self, url, playerId, gameId):
        super().__init__(url, playerId, gameId)

    def join(self):
        return get(f'{self.url}/train/play?playerID={self.playerId}&gameID={self.gameId}')

    def getDoActionUrl(self, action):
        return f'{self.url}/train/doAction?playerID={self.playerId}&gameID={self.gameId}&action={action}'


class RealClient(AbstractClient):
    def __init__(self, url, playerId, gameId):
        super().__init__(url, playerId, gameId)

    def join(self):
        return get(f'{self.url}/game/play?playerID={self.playerId}&gameID={self.gameId}')

    def getDoActionUrl(self, action):
        return f'{self.url}/doAction?playerID={self.playerId}&gameID={self.gameId}&action={action}'


class Player:
    def __init__(self, client):
        self.client = client
        self.initProgress = 0
        print(self.client.join())

    def play(self):
        if self.initProgress < 2:
            print(f"'i x y' for initial\n")
            action = input()
            pts = action.split()
            if pts[0] == 'i':
                self.initProgress += 1
                self.client.doInitial(int(pts[1]), int(pts[2]))
        else:
            print(f"'m x' for move\n'br x' for road\n'bt' for town\n'ut x' for upgrade town\n'e' for empty\n")
            action = input()
            pts = action.split()
            if pts[0] == 'm':
                self.client.doMove(int(pts[1]))
            elif pts[0] == 'br':
                self.client.doBuildRoad(int(pts[1]))
            elif pts[0] == 'bt':
                self.client.doBuildTown()
            elif pts[0] == 'ut':
                self.client.doUpgradeTown(int(pts[1]))
            elif pts[0] == 'e':
                self.client.doEmpty()


class InitialStrategy:
    def __init__(self, map):
        self.map = map

    def getFirstInitial(self):
        scores = sorted([(self.getFirstScore(ind), ind) for ind in range(96)], reverse=True)
        for _, idx in scores:
            if self.isValidMove(idx):
                return idx, self.getFirstRoad(idx)

    def getSecondInitial(self):
        hasWheat = self.map.wheatGen > 0
        hasWood = self.map.woodGen > 0
        hasSheep = self.map.sheepGen > 0
        hasClay = self.map.clayGen > 0

        valids = self.getRequired(hasWheat, hasWood, hasSheep, hasClay)
        scores = sorted([(self.getSecondScore(ind), ind) for ind in valids], reverse=True)
        for _, idx in scores:
            if self.isValidMove(idx):
                return idx, self.getFirstRoad(idx)

    def getRequired(self, wheat, wood, sheep, clay):
        valids = []
        for idx in range(96):
            w = wheat
            wo = wood
            s = sheep
            c = clay
            inte = self.map.getIntersection(idx)
            for tile in inte.tiles:
                if tile.type == 2:
                    s = True
                elif tile.type == 3:
                    wo = True
                elif tile.type == 4:
                    w = True
                elif tile.type == 5:
                    c = True
            if w and wo and s and c:
                valids.append(idx)
        return valids

    def getFirstRoad(self, idx):
        roades = self.map.getValidRoades(idx)
        return sorted([(abs(road - 40), road) for road in roades])[0][1]

    def isValidMove(self, ind):
        if not self.map.canBuildTown(ind, True):    return False
        inte = self.map.getIntersection(ind)
        if len(inte.roads) == 3:    return False
        return True

    def getFirstScore(self, index):
        inte = self.map.getIntersection(index)
        result = 0
        pts = set()
        for tile in inte.tiles:
            if tile.type == 2:
                pts.add(2)
                result += tile.weight * 0.6
            elif tile.type == 3:
                pts.add(3)
                result += tile.weight
            elif tile.type == 4:
                pts.add(4)
                result += tile.weight * 0.6
            elif tile.type == 5:
                pts.add(5)
                result += tile.weight
            elif tile.type == 6:
                result += tile.weight * 0.2
        if len(pts) >= 2:
            result *= 2

        return result

    def getSecondScore(self, index):
        inte = self.map.getIntersection(index)
        result = 0
        for tile in inte.tiles:
            if tile.type == 2:
                result += tile.weight
            elif tile.type == 3:
                result += tile.weight
            elif tile.type == 4:
                result += tile.weight
            elif tile.type == 5:
                result += tile.weight
            elif tile.type == 6:
                result += tile.weight * 0.8
        return result


class GameStrategy:
    def __init__(self, map):
        self.map = map

    def getAction(self):
        pass


class SmartPlayer:
    def __init__(self, client):
        self.client = client
        self.initProgress = 0
        self.visited = []

        joinResp = self.client.join()
        action = joinResp['result']['action']

        self.map = Map(joinResp['result'])
        self.initialStrategy = InitialStrategy(self.map)

        if action:
            pts = action.split()
            self.map.viBuilder = int(pts[1])
            self.map.buildCity(int(pts[1]), False)
            self.map.buildRoad(int(pts[1]), int(pts[2]), False)

    def calculateIntersectionGoodness(self, idx):
        tiles = self.map.getIntersection(idx).tiles

        weightedSum = 0
        for tile in tiles:
            resourceType = tile.type
            resourceWeight = tile.weight
            if resourceType == 2:
                weightedSum += resourceWeight * 0.7
            elif resourceType == 3:
                weightedSum += resourceWeight
            elif resourceType == 4:
                weightedSum += resourceWeight * 0.7
            elif resourceType == 5:
                weightedSum += resourceWeight
            elif resourceType == 6:
                weightedSum += resourceWeight * 0.85

        if any([x.type == 0 for x in tiles]) or any([x.type == 1 for x in tiles]):
            weightedSum /= 2

        if idx in self.visited:
            weightedSum /= 3

        return weightedSum - abs(40 - idx)

    def play(self):
        self.initProgress += 1
        if self.initProgress < 3:
            if self.initProgress == 1:
                town, road = self.initialStrategy.getFirstInitial()
            else:
                town, road = self.initialStrategy.getSecondInitial()
            response = self.client.doInitial(town, road)
            if self.map.miBuilder is None:
                self.map.miBuilder = town
            self.map.buildCity(town, True, True)
            self.map.buildRoad(town, road, True, True)
            if response['result'] != f'initial {town} {road}':
                self.handleOpponentResponse(response)
        else:
            neighbours = self.map.getIntersection(self.map.miBuilder).neighbors
            goodnesses = list(map(self.calculateIntersectionGoodness, neighbours))
            if goodnesses:
                direction = neighbours[goodnesses.index(max(goodnesses))]
            else:
                for neighbour in neighbours:
                    if self.map.canMove(neighbour):
                        response = self.client.doMove(neighbour)
                        self.handleOpponentResponse(response)
                        return
                response = self.client.doMove(random.randint(0, len(neighbours) - 1))
                self.handleOpponentResponse(response)
                return

            myBestTown = self.map.getMyBestTown()
            if myBestTown is not None and self.map.canUpgradeTown(myBestTown.index):
                response = self.client.doUpgradeTown(myBestTown.index)
                self.map.upgradeTown(myBestTown.index)
                self.map.generateResources()
                self.map.generateResources()
                self.handleOpponentResponse(response)
                return

            if self.map.canBuildTown(self.map.miBuilder, True):
                if self.map.canBuildTown(self.map.miBuilder, False):
                    response = self.client.doBuildTown()
                    self.map.buildCity(self.map.miBuilder, True)
                else:
                    response = self.client.doEmpty()
                self.handleOpponentResponse(response)
                self.map.generateResources()
                self.map.generateResources()
                return

            while direction in self.visited:
                neighbours.remove(direction)
                if len(neighbours) == 0:
                    direction = self.visited[-1]
                    self.visited.pop()
                    break
                goodnesses = list(map(self.calculateIntersectionGoodness, neighbours))
                direction = neighbours[goodnesses.index(max(goodnesses))]

            if self.map.isRoadBlocked(direction):
                for n in neighbours:
                    if self.map.canBuildRoad(n):
                        direction = n
                        break

            if not self.map.isRoadBlocked(direction):
                if self.map.canBuildRoad(direction):
                    response = self.client.doBuildRoad(direction)
                    self.handleOpponentResponse(response)
                    self.map.buildRoad(direction, self.map.miBuilder, True)
                else:
                    response = self.client.doEmpty()
                    self.handleOpponentResponse(response)
            else:
                miBuilderPosition = self.map.getIntersection(self.map.miBuilder)
                if (direction, True) in miBuilderPosition.roads:
                    self.visited.append(self.map.miBuilder)
                    response = self.client.doMove(direction)
                    self.handleOpponentResponse(response)
                    self.map.move(direction)
                else:
                    rds = self.map.getMyRoades(self.map.miBuilder)
                    if len(rds):
                        self.visited.append(self.map.miBuilder)
                        response = self.client.doMove(rds[random.randint(0, len(rds) - 1)])
                        self.handleOpponentResponse(response)
                        self.map.move(rds[random.randint(0, len(rds) - 1)])
                    else:
                        if len(self.visited):
                            dd = self.visited.pop()
                            self.visited.append(self.map.miBuilder)
                            response = self.client.doMove(dd)
                            self.handleOpponentResponse(response)
            self.map.generateResources()
            self.map.generateResources()

    def handleOpponentResponse(self, response):
        print(response)
        pts = response['result'].split()
        if self.map.viBuilder is None:
            self.map.viBuilder = int(pts[1])
            self.map.buildCity(int(pts[1]), False)
            self.map.buildRoad(int(pts[1]), int(pts[2]), False)
            if len(pts) == 6:
                self.map.buildCity(int(pts[4]), False)
                self.map.buildRoad(int(pts[4]), int(pts[5]), False)
        else:
            if pts[0] == "initial":
                self.map.buildCity(int(pts[1]), False)
                self.map.buildRoad(int(pts[1]), int(pts[2]), False)
            elif pts[0] == "move":
                self.map.enemyMove(int(pts[1]))
            elif pts[0] == "buildroad":
                self.map.buildRoad(int(pts[1]), self.map.viBuilder, False)
            elif pts[0] == "buildtown":
                self.map.buildCity(self.map.viBuilder, False)
            elif pts[0] == "upgradetown":
                self.map.upgradeTown(self.map.viBuilder)
            else:
                pass


def commandLine(ip, id):
    player1 = SmartPlayer(RealClient(f"http://{ip}:9080/", id, 1))

    while True:
        player1.play()


if __name__ == '__main__':
    if len(argv) != 3:
        print('IP and ID missing')
    ip = argv[1]
    id = argv[2]
    commandLine(ip, id)
