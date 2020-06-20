# KLIENT

import socket
import random

def is_prime(x):
    count = 0
    for i in range(int(x/2)):
        if x % (i+1) == 0:
            count += 1
        if count > 1:
            break
    if count==1:
        return 1
    else:
        return count

def prime_number():
    while True:
        x = random.randint(100, 500)
        print("LICZBA wylosowana: " + str(x))
        odp = is_prime(x)
        if odp==1:
            print("ZWRACAM LICZE: ", x)
            return x

def gcd(a,b):
    while b != 0:
        a, b = b, a % b
    return a

def primitive_root(modulo):
    roots = []
    required_set = set(num for num in range (1, modulo) if gcd(num, modulo) == 1)

    for g in range(1, modulo):
        actual_set = set(pow(g, powers) % modulo for powers in range (1, modulo))
        if required_set == actual_set:
            roots.append(g)
    return roots[0]



server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

try:
    server.connect(('localhost', 1337))
    strasd="HELLO"
    content_length = len(strasd)
    server.sendall(("0/JD/1.1\r\n0000\r\n" + str(len(strasd)) + "\r\n\r\nHELLO").encode())
    data = server.recv(4096)
    if data:
        print(data.decode())
    else:
        print("Siemanoelo")

    sp = int(data.split()[4])
    sg = int(data.split()[5])

    secret_key = random.randint(5, 1000)
    result_DH = (sg ** secret_key) % sp

    server_result = int(data.split()[6])
    print("P=" + str(sp) + " G=" + str(sg) + " SERVER_RESULT=" + str(server_result))
    print(server_result**secret_key)
    s = server_result**secret_key%sp
    print("MOJE S wynosi: ", s)

    server.sendall(("1/JD/1.1\r\n0\r\n" + str(len(str(result_DH))) + "\r\n\r\n" + str(result_DH)).encode())
    data = server.recv(4096)
    if data:
        print(data.decode())
    else:
        print("Siemanoelo")


except:
    print("siema")