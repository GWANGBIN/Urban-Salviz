import socketserver
import socket
from queue import Queue
from _thread import *
import numpy as np
import tensorflow as tf
config = tf.ConfigProto()
config.gpu_options.allow_growth = True
session = tf.Session(config=config)
import cv2
import os

os.environ["TF_CPP_MIN_LOG_LEVEL"] = "2"
tf.logging.set_verbosity(tf.logging.ERROR)
model_fn = "model_salicon_gpu.pb"

enclosure_queue = Queue()
cap = cv2.VideoCapture(1)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 320)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 240)
cap.set(cv2.CAP_PROP_FPS, 30)

def capture(queue):
    graph_def = tf.GraphDef()
    with tf.gfile.Open(model_fn, "rb") as file:
        graph_def.ParseFromString(file.read())
    input_plhd = tf.placeholder(tf.float32, (None, None, None, 3))
    [predicted_maps] = tf.import_graph_def(graph_def, input_map={"input": input_plhd}, return_elements=["output:0"])


    with tf.Session() as sess:
        try:
            while True:
                ret, frame = cap.read()
                image = cv2.resize(frame, (320, 240))
                img = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
                img = img[np.newaxis, :, :, :]
                saliency = sess.run(predicted_maps, feed_dict={input_plhd: img})
                saliency = cv2.cvtColor(saliency.squeeze(), cv2.COLOR_GRAY2BGR)
                saliency = np.uint8(saliency * 255)
                saliency = cv2.resize(saliency, (320, 240))
                saliency = cv2.cvtColor(saliency, cv2.COLOR_BGR2GRAY)
                saliency = cv2.applyColorMap(saliency, cv2.COLORMAP_JET)
                dst = cv2.addWeighted(image, 0.6, saliency, 0.4, 0)

                target_frame = saliency
                cv2.imshow('saliency', dst)


                encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), 80]  # 0 ~ 100 quality
                result, imgencode = cv2.imencode('.jpg', target_frame, encode_param)  # Encode numpy into '.jpg'
                data = np.array(imgencode)
                stringData = data.tostring()  # Convert numpy to string
                queue.put(stringData)  # Put the encode in the queue stack
                print("succesfully uploaded")
                if cv2.waitKey(1) > 0: break

        finally:
            cap.release()
            cv2.destroyAllWindows()


class MyTCPHandler(socketserver.BaseRequestHandler):
    queue = enclosure_queue
    stringData = str()

    def handle(self):

        # 'self.request' is the TCP socket connected to the client
        print("A client connected by: ", self.client_address[0], ":", self.client_address[1])

        while True:
            try:
                # _server <- client
                self.data = self.request.recv(1024).strip()  # 1024 byte for header
                # print("Received from client: ", self.data)

                if not self.data:
                    print("The client disconnected by: ", self.client_address[0], ":", self.client_address[1])
                    break

                    # _Get data from Queue stack
                MyTCPHandler.stringData = MyTCPHandler.queue.get()

                # _server -> client
                # print(str(len(MyTCPHandler.stringData)).ljust(16).encode())  # <str>.ljust(16) and encode <str> to <bytearray>

                ###self.request.sendall(str(len(MyTCPHandler.stringData)).ljust(16).encode())  # <- Make this line ignored when you connect with C# client.
                self.request.sendall(MyTCPHandler.stringData)

                # self.request.sendall(len(MyTCPHandler.stringData).to_bytes(1024, byteorder= "big"))
                # self.request.sendall(MyTCPHandler.stringData)


            except ConnectionResetError as e:
                print("The client disconnected by: ", self.client_address[0], ":", self.client_address[1])
                break


if __name__ == "__main__":

    # _Webcam process is loaded onto subthread
    start_new_thread(capture, (enclosure_queue,))

    # _Server on
    HOST, PORT = socket.gethostname(), 8080
    with socketserver.TCPServer((HOST, PORT), MyTCPHandler) as server:

        print("****** Server started ****** ", end="\n \n")

        try:
            server.serve_forever()

        except KeyboardInterrupt as e:
            print("******  Server closed ****** ", end="\n \n")
